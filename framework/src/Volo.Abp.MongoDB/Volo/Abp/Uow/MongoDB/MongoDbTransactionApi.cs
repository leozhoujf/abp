using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Volo.Abp.Uow.MongoDB
{
    public class MongoDbTransactionApi : ITransactionApi, ISupportsRollback
    {
        public IClientSessionHandle SessionHandle { get; }
        
        public MongoDbTransactionApi(IClientSessionHandle sessionHandle)
        {
            SessionHandle = SessionHandle;
        }

        public void Commit()
        {
            SessionHandle.CommitTransaction();
        }

        public async Task CommitAsync()
        {
            await SessionHandle.CommitTransactionAsync();
        }
        
        public void Dispose()
        {
            SessionHandle.Dispose();
        }
        
        public void Rollback()
        {
            SessionHandle.AbortTransaction();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await SessionHandle.AbortTransactionAsync(cancellationToken);
        }
    }
}