using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using XOFF.Core;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;
using XOFF.Core.Repositories;
using XOFF.Core.Repositories.Settings;

namespace XOFF.Tests.Framework.Core.Repositories
{
	[TestFixture()]
	public class SyncedRepositoryTests
	{
	    private Mock<IRepository<Person, Guid>> _mockRepository;
	    private SyncRepositorySettings _settings;
	    private Mock<IRemoteEntityGetter<Person, Guid>> _mockRemoteEntityGetter;
	    private Mock<IConnectivityChecker> _mockConnectivityChecker;
	    private Mock<IChangeQueue<Person, Guid>> _mockChangeQueue;

	    [SetUp]
	    public void Setup()
	    {
            _mockRepository = new Mock<IRepository<Person, Guid>>();
            _settings = new SyncRepositorySettings(86400, RefreshDataMode.RefreshIfStale);
            _mockRemoteEntityGetter = new Mock<IRemoteEntityGetter<Person, Guid>>();
            _mockConnectivityChecker = new Mock<IConnectivityChecker>();
            _mockChangeQueue = new Mock<IChangeQueue<Person, Guid>>();//nothing to setup here 
        }

	    [TearDown]
	    public void TearDown()
	    {
	        _mockRepository = null;
            _settings = null;
            _mockRemoteEntityGetter = null;
            _mockConnectivityChecker = null;
            _mockChangeQueue = null;
        }

	    [Test]
		public async Task Get_RefreshIfStale_ItemsNotStale_ReturnsItems()
        {
            //Setep

            DateTime itemDate = DateTime.UtcNow;
            List<Person> people = GetPeople(itemDate);

            _mockRepository = new Mock<IRepository<Person, Guid>>();
            _mockRepository.Setup(m => m.All(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<Func<IQueryable<Person>, IOrderedQueryable<Person>>>(),
                It.IsAny<bool>())).Returns(OperationResult<IList<Person>>.CreateSuccessResult(people));

            _settings = new SyncRepositorySettings(86400, RefreshDataMode.RefreshIfStale);


            _mockRemoteEntityGetter = new Mock<IRemoteEntityGetter<Person, Guid>>();
            _mockRemoteEntityGetter.Setup(x => x.Get())
                .Throws(new Exception("This method should not have been called in this case"));

            _mockConnectivityChecker = new Mock<IConnectivityChecker>();
            _mockConnectivityChecker.Setup(x => x.Connected).Returns(true);

            _mockChangeQueue = new Mock<IChangeQueue<Person, Guid>>();//nothing to setup here shouldn


            var syncedRepository = new SyncedRepository<Person, Guid>(_mockRepository.Object, _settings, _mockRemoteEntityGetter.Object, _mockConnectivityChecker.Object, _mockChangeQueue.Object);

            //Act
            var result = await syncedRepository.Get();
            //Assert
            Debug.WriteLine(result.Result.Count.ToString());
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result.Count == people.Count);
            foreach (var person in people)
            {
                Assert.IsTrue(result.Result.Any(x => x.Id == person.Id));
            }
        }

	    [Test]
	    public async Task Get_RefreshIfStale_ItemsStale_NewItemsRetreivedAndRetrieved()
	    {
	        //Setep
	        _mockConnectivityChecker.Setup(x => x.Connected).Returns(true);

	        var localPeople = GetPeople(DateTime.UtcNow - TimeSpan.FromDays(10));

            _mockRepository.Setup(m => m.All(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<Func<IQueryable<Person>, IOrderedQueryable<Person>>>(),
               It.IsAny<bool>())).Returns(OperationResult<IList<Person>>.CreateSuccessResult(localPeople));

            var remotePeopleSyncTime = DateTime.UtcNow;
            IList<Person> remotePeople = GetPeople(remotePeopleSyncTime);

	        _mockRepository.Setup(x => x.ReplaceAll(It.IsAny<ICollection<Person>>())).Callback(() =>
	        {
	            _mockRepository.Setup(m => m.All(
	                It.IsAny<Expression<Func<Person, bool>>>(),
	                It.IsAny<Func<IQueryable<Person>, IOrderedQueryable<Person>>>(),
	                It.IsAny<bool>())).Returns(OperationResult<IList<Person>>.CreateSuccessResult(remotePeople));
	        });

            OperationResult <IList<Person>> remotePeopleResult = OperationResult<IList<Person>>.CreateSuccessResult(remotePeople);
	        _mockRemoteEntityGetter.Setup(x => x.Get()).Returns(()=>Task.FromResult(remotePeopleResult));

            var syncedRepository = new SyncedRepository<Person, Guid>(_mockRepository.Object, _settings, _mockRemoteEntityGetter.Object, _mockConnectivityChecker.Object, _mockChangeQueue.Object);


            //Act
	        var result = await syncedRepository.Get();
            //Assert
           
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result.Count == remotePeople.Count);
            foreach (var person in remotePeople)
            {
                Assert.IsTrue(result.Result.Any(x => x.Id == person.Id && x.LastTimeSynced == remotePeopleSyncTime));
            }
        }

        [Test]
        public async Task Get_OnlyOnRefresh_ItemsStale_NewItemsRetreivedAndRetrieved()
        {
            //Setep
            _settings = new SyncRepositorySettings(refreshDataMode:RefreshDataMode.OnlyOnRefresh);

            _mockConnectivityChecker.Setup(x => x.Connected).Returns(true);
            var localPeopleTime = DateTime.UtcNow - TimeSpan.FromDays(10);
            var localPeople = GetPeople(localPeopleTime);

            _mockRepository.Setup(m => m.All(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<Func<IQueryable<Person>, IOrderedQueryable<Person>>>(),
               It.IsAny<bool>())).Returns(OperationResult<IList<Person>>.CreateSuccessResult(localPeople));

            var remotePeopleSyncTime = DateTime.UtcNow;
            IList<Person> remotePeople = GetPeople(remotePeopleSyncTime);

            _mockRepository.Setup(x => x.ReplaceAll(It.IsAny<ICollection<Person>>())).Callback(() =>
            {
                Assert.Fail();
            });

            OperationResult<IList<Person>> remotePeopleResult = OperationResult<IList<Person>>.CreateSuccessResult(remotePeople);
            _mockRemoteEntityGetter.Setup(x => x.Get()).Returns(() => Task.FromResult(remotePeopleResult));

            var syncedRepository = new SyncedRepository<Person, Guid>(_mockRepository.Object, _settings, _mockRemoteEntityGetter.Object, _mockConnectivityChecker.Object, _mockChangeQueue.Object);

            //Act
            var result = await syncedRepository.Get();
            //Assert

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result.Count == localPeople.Count);
            foreach (var person in localPeople)
            {
                Assert.IsTrue(result.Result.Any(x => x.Id == person.Id && x.LastTimeSynced == localPeopleTime));
            }
        }

        [Test]
        public async Task Get_RepositoryReturnsFailure_ReturnFailure()
        {
            //Setep
            _settings = new SyncRepositorySettings(refreshDataMode: RefreshDataMode.OnlyOnRefresh);

            _mockConnectivityChecker.Setup(x => x.Connected).Returns(true);

            _mockRepository.Setup(m => m.All(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<Func<IQueryable<Person>, IOrderedQueryable<Person>>>(),
               It.IsAny<bool>())).Returns(OperationResult<IList<Person>>.CreateFailure("failed"));

            var remotePeopleSyncTime = DateTime.UtcNow;
            IList<Person> remotePeople = GetPeople(remotePeopleSyncTime);

            OperationResult<IList<Person>> remotePeopleResult = OperationResult<IList<Person>>.CreateSuccessResult(remotePeople);
            _mockRemoteEntityGetter.Setup(x => x.Get()).Returns(() => Task.FromResult(remotePeopleResult));

            var syncedRepository = new SyncedRepository<Person, Guid>(_mockRepository.Object, _settings, _mockRemoteEntityGetter.Object, _mockConnectivityChecker.Object, _mockChangeQueue.Object);

            //Act
            var result = await syncedRepository.Get();
            //Assert

            Assert.IsFalse(result.Success);
        }


        private static List<Person> GetPeople(DateTime itemDate)
        {
            return new List<Person>()
            {
                new Person()
                {
                    Id = Guid.NewGuid(),
                    LastTimeSynced = itemDate
                },
                new Person()
                {
                    Id = Guid.NewGuid(),
                    LastTimeSynced = itemDate
                },
                new Person()
                {
                    Id = Guid.NewGuid(),
                    LastTimeSynced = itemDate
                },
                new Person()
                {
                    Id = Guid.NewGuid(),
                    LastTimeSynced = itemDate
                }
            };
        }
    }
}
