namespace Passi.Core.Application.Repositories
{
    internal interface IInfoRepository<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<T> RetrieveAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<T> UpdateAsync(T item);
    }
}
