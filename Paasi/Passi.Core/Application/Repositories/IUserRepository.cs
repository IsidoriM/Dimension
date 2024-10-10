using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Core.Application.Repositories
{
    internal interface IUserRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authType"></param>
        /// <param name="institutionCode"></param>
        /// <returns></returns>
        public Task<UserInfo> UserAsync(string id, string authenticationType, string institutionCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<bool> IsDelegationAvailableAsync(string fiscalCode, string institutionCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="uniqueSessionsCount"></param>
        /// <returns></returns>
        public Task<bool> HasSessionAsync(string userId, string sessionId, int uniqueSessionsCount);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceId"></param>
        /// <param name="userTypeId"></param>
        /// <param name="authenticationType"></param>
        /// <param name="retrieveSuspendedProfiles"></param>
        /// <returns></returns>
        public Task<ICollection<Profile>> ProfilesAsync(string userId,
            int serviceId,
            int? userTypeId,
            string authenticationType = CommonAuthenticationTypes.Undefined,
            bool retrieveSuspendedProfiles = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="delegatedUserId"></param>
        /// <returns></returns>
        public Task<bool> HasDelegationAsync(string userId, string delegatedUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="institutionCode"></param>
        /// <returns></returns>
        public Task<ICollection<Convention>> ConventionsAsync(string userId, string institutionCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="institutionCode"></param>
        /// <param name="authenticationType"></param>
        /// <param name="retrieveSuspendedProfiles"></param>
        /// <returns></returns>
        public Task<ICollection<Service>> AuthorizedServicesAsync(string userId,
            string institutionCode,
            string authenticationType,
            bool retrieveSuspendedProfiles = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="institutionCode"></param>
        /// <returns></returns>
        public Task<ICollection<Service>> ServicesAsync(string userId, string institutionCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceId"></param>
        /// <param name="institutionCode"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public Task<bool> IsGrantedAsync(string userId,
            int serviceId,
            string institutionCode,
            string authenticationType = CommonAuthenticationTypes.Undefined);
    }
}
