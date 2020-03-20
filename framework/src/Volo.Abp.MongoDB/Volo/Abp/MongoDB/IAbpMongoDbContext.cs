using MongoDB.Driver;

namespace Volo.Abp.MongoDB
{
    public interface IAbpMongoDbContext
    {
        IMongoDatabase Database { get; }
        
        IClientSessionHandle SessionHandle { get; }

        IMongoCollection<T> Collection<T>();
    }
}