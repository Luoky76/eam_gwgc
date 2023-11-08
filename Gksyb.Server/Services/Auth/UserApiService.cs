using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.UI;
using System.Linq.Expressions;

namespace Gksyb.Server.Services.Auth
{
    public partial class UserService : IUserService
    {
        /// <inheritdoc/>
        public async Task<List<ComboxData>> ComboxDataAsync(string corp = null)
        {
            return await _dbContext.Query<CF_USER>().Where(FilterSuper(_options))
                .Select(c => new ComboxData()
                {
                    ID = c.USERID,
                    TEXT = c.REALNAME,
                    VALUE = c.DEPARTCODE
                }).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ComboxData>> GroupsAsync()
        {
            var groups = await _dbContext.Query<CF_USER>().Where(FilterSuper(_options)).Select(c => c.STATION).Distinct().ToListAsync();
            return groups.DistinctAndOrderBy().Select(c => new ComboxData()
            {
                ID = c,
                TEXT = c,
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> AllUsers()
        {
            return await GetUsers();
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> Users(bool filterSelf = true)
        {
            return await GetUsers(query =>
            {
                if (filterSelf) query = query.Where(c => c.USERID != _user.UserID);
                return FilterCorp(query);
            });
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> Find(List<long?> ids, bool skipCorp = true)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options))
               .Where(c => ids.Contains(c.USERID))
               .Select(UserInfoExtensions.SelectUserInfo);
            var users = await query.ToListAsync();
            if (users.Count < 1) return users;
            if (skipCorp) return users;
            await HandleCorp(users);
            return users;
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindUsersAsync(Expression<Func<UserInfo, bool>> filter = null, bool skipCorp = true)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options));
            var users = await query.Select(UserInfoExtensions.SelectUserInfo)
                .WhereIfNotNull(filter, filter).ToListAsync();
            if (users.Count < 1) return users;
            if (skipCorp) return users;
            await HandleCorp(users);
            return users;
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindOperators(FindOperatorInfo info)
        {
            var operators = (info.Operators ?? "").Split(UserInfo.DefaultSplit).DistinctAndOrderBy().ToList();
            var users = new List<UserInfo>();
            if (operators.Count < 1) return users;
            _hasSuper = info.HasSuper;
            switch (info.Type)
            {
                case "Station":
                    if (string.IsNullOrWhiteSpace(info.Corp)) return users;
                    foreach (var station in operators)
                    {
                        users.AddRange(await FindByStation(info.Corp, station));
                    }
                    break;

                case "CoprStation":
                    if (string.IsNullOrWhiteSpace(info.Corp)) return users;
                    foreach (var station in operators)
                    {
                        users.AddRange(await FindByCorpStation(info.Corp, station));
                    }
                    break;

                case "Group":
                    users.AddRange(await FindByGroup(operators));
                    break;

                case "Role":
                    users.AddRange(await FindByRole(operators.Select(c => c.CastTo<long?>()).ToList()));
                    break;

                case "User":
                    users.AddRange(await Find(operators.Select(c => c.CastTo<long?>()).ToList()));
                    break;
            }
            users = users.DistinctBy(c => c.Id).ToList();
            return users;
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindByStation(string CorpId, string station, bool skipCorp = true)
        {
            var users = await FindByCorp(CorpId, station, skipCorp);
            if (users.Count > 0) return users;
            var parentId = await _dbContext.Query<CF_CORP>().Where(a => a.CORPID == CorpId).Select(a => a.CORPPARENTID).FirstOrDefaultAsync();
            if (parentId == null) return users;
            users = await FindByStation(parentId, station, skipCorp);
            return users;
        }

        /// <summary>
        /// 查找上级公司指定岗位的人员
        /// </summary>
        public async Task<List<UserInfo>> FindByCorpStation(string CorpId, string station)
        {
            var parentId = await _dbContext.Query<CF_CORP>().Where(a => a.CORPID == CorpId).Select(a => a.CORPPARENTID).FirstOrDefaultAsync();
            return await FindByStation(parentId, station);
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindByGroup(List<string> groups, bool skipCorp = true)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options))
                .Where(c => groups.Contains(c.STATION))
                .Select(UserInfoExtensions.SelectUserInfo);
            var users = await query.ToListAsync();
            if (users.Count < 1) return users;
            if (skipCorp) return users;
            await HandleCorp(users);
            return users;
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindByRole(List<long?> roles, bool skipCorp = true)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options))
                .Where(c => _dbContext.Query<CF_USERROLE>().Where(a => a.USERID == c.USERID && roles.Contains(a.ROLEID) && a.APPNAME == _options.RoleAppName).Any())
                .Select(UserInfoExtensions.SelectUserInfo);
            var users = await query.ToListAsync();
            if (users.Count < 1) return users;
            if (skipCorp) return users;
            await HandleCorp(users);
            return users;
        }

        /// <inheritdoc/>
        public async Task<List<UserInfo>> FindByCorp(string CorpId, string station = null, bool skipCorp = false)
        {
            IQuery<CF_USER> filter(IQuery<CF_USER> query) => query.Where(c => _dbContext.Query<CF_USER_PORT>().Where(a => a.LOGINNAME == c.LOGINNAME && a.APPNAME == c.APPNAME && a.CORPID == CorpId && a.OPTYPE == _opertype)
                 .WhereIfNotNullOrEmpty(station, a => (UserInfo.DefaultSplit + a.REMARK + UserInfo.DefaultSplit).Contains($"{UserInfo.DefaultSplit}{station}{UserInfo.DefaultSplit}")).Any());
            return await GetUsers(filter);
        }

        /// <summary>
        /// 根据条件获取用户
        /// </summary>
        private async Task<List<UserInfo>> GetUsers(Func<IQuery<CF_USER>, IQuery<CF_USER>> filter = null, bool skipCorp = false)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options));
            if (filter != null) query = filter(query);
            var users = await query.Select(UserInfoExtensions.SelectUserInfo).ToListAsync();
            if (users.Count < 1) return users;
            if (skipCorp) return users;
            await HandleCorp(users);
            return users;
        }

        /// <summary>
        /// 处理UserInfo的公司和岗位
        /// </summary>
        private async Task HandleCorp(List<UserInfo> users)
        {
            var corps = await _dbContext.Query<CF_CORP>().Where(a => a.VALIDFLAG == "1").Select(CorpInfoExtensions.SelectCorpInfo).ToListAsync();
            var userPorts = await _dbContext.Query<CF_USER_PORT>().Where(a => a.OPTYPE == _opertype && a.APPNAME == _options.UserAppName).ToListAsync();
            foreach (var user in users)
            {
                var ports = userPorts.Where(a => a.LOGINNAME == user.Account).ToList();
                ports.Where(a => a.OPTYPE == _opertype).ForEach(a =>
                {
                    var corp = corps.Find(c => c.CorpID == a.CORPID);
                    if (corp == null) return;
                    corp.Station = (a.REMARK ?? "").Split(",").DistinctAndOrderBy().ToList();
                    user.Corps.Add(corp);
                });
            }
        }
    }
}