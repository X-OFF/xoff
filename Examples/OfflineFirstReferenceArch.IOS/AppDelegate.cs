using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Foundation;
using OfflineFirstReferenceArch.ViewModels;
using OfflineFirstReferenceArch.Widgets;
using OfflineFirstReferenceArch.Models;
using UIKit;
using XOFF.Autofac;
using XOFF.Core.Remote;
using LiteDB;
using System.Diagnostics;
using XOFF.Core;
using System.Threading.Tasks;
using System.Threading;
using Autofac.Core;
using XOFF.Core.Remote.Http;
using XOFF.iOS.Reachability;
using XOFF.iOS.DI;

namespace OfflineFirstReferenceArch.IOS
{
    public static class DIContainer
    {
        public static IContainer ContainerInstance { get; set; }
    }


    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
		CancellationTokenSource _cancellationToken;

		Task _processQueueTask;
        private IRemoteEntityRefreshCoordinator _refreshCoordinator;

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create our container builder
            var builder = new ContainerBuilder();


            RegisterDependencies(builder);
            DIContainer.ContainerInstance = builder.Build();

            // return true
            return true;
        }

        private void RegisterDependencies(ContainerBuilder builder)
        { 	//XOFF
            builder.RegisterType<XOFFHttpEntityGetter<Widget,Guid>>().As<IRemoteEntityGetter<Widget, Guid>>();

			RegisterLiteDbDependencies(builder);
            RegisteriOSDependencies(builder);

			//services, getters, etc. 
			builder.RegisterType<WidgetReader>().As<IWidgetReader>();
            builder.RegisterType<WidgetCreator>().As<IWidgetCreator>();
            builder.RegisterType<WidgetDeleter>().As<IWidgetDeleter>();

            builder.RegisterType<XOFFHttpClientProvider>().As<IHttpClientProvider>().WithParameter("baseUrl", "https://api.backendless.com/2EB98F53-B604-4858-FF6C-D9D39616C700/4FA7BB59-F932-7688-FF16-F25B663F9900/data/"); 
            builder.RegisterType<XOFFHttpEntityCreateHandler<Widget,Guid>>().As<IRemoteEntityCreateHandler<Widget,Guid>>().WithParameter("endpointUri", "widgets");

         
            var widgetGetParamenters = new List<Parameter>()
            {
                new NamedParameter("getAllEndPointUri","widgets"),
                new NamedParameter("getOneFormatString","widgets/{0}"),
            }; 

            builder.RegisterType<XOFFHttpEntityGetter<Widget, Guid>>()
                .As<IRemoteEntityGetter<Widget, Guid>>()
                .WithParameters(widgetGetParamenters);

            builder.RegisterType<XOFFHttpEntityDeleteHandler<Widget, Guid>>()
                .As<IRemoteEntityDeleteHandler<Widget, Guid>>()
                .WithParameter("endPointFormatString", "widgets/{0}");

            //viewmodels 
            builder.RegisterType<LandingPageViewModel>();
        }

		public void RegisterLiteDbDependencies(ContainerBuilder builder ) 
		{
			builder.RegisterModule<XOFFLiteDbAutoFacModule>();
		}

        public void RegisteriOSDependencies(ContainerBuilder builder)
        {
            builder.RegisterModule<XoffAutoFaciOSModule>();
        }
	
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {

			 Window = new UIWindow(UIScreen.MainScreen.Bounds);
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            var rootViewController = UIStoryboard.FromName("Main", null).InstantiateInitialViewController();
            Window.RootViewController = rootViewController;
            Window.MakeKeyAndVisible();

		
			_cancellationToken = new CancellationTokenSource();
			  
			_processQueueTask = Task.Factory.StartNew(ProcessQueue, _cancellationToken.Token);

            _refreshCoordinator = DIContainer.ContainerInstance.Resolve<IRemoteEntityRefreshCoordinator>();
            _refreshCoordinator.RegisterTypeToRefresh(typeof(Widget),typeof(Guid));
            _refreshCoordinator.Start();
            return true;
        }

		void ProcessQueue()
		{

			while (!_cancellationToken.Token.IsCancellationRequested)
			{
			    try
			    {
                    var queueProcessor = DIContainer.ContainerInstance.Resolve<IQueueProcessor>();

			        queueProcessor.ProcessQueue().Wait();
			        Debug.WriteLine("Processed Queue");
                }
				catch (Exception ex) 
				{
                    Debug.WriteLine($"Exception throw: {ex.Message}");
				}
			  	Task.Delay(5000).Wait();
			}

		}


		public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }	
    }
}


