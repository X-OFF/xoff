namespace XOFF.Core.Remote
{
	public interface IRemoteEntityDeleteHandler<TModel, TIdentifier> : IRemoteDeleteHandler where TModel : IModel<TIdentifier>
	{
		
	}
}