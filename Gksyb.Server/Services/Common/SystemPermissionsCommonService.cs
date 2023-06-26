using Chloe;
using Gksyb.Common;
using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Common;
using Gksyb.Core.Filter;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Common
{
    public class SystemPermissionsCommonService:ISystemPermissionsCommonService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession CurrentUser;

        public SystemPermissionsCommonService(IDbContext dbContext, UserSession currentUser)
        {
            _dbContext = dbContext;
            CurrentUser = currentUser;
        }

        public async Task<AjaxResult> GetCurrentCorp()
        {
            var corp = CurrentUser.Corp;

            if (null == corp)
            {
                throw new MessageException("获取当前登录人信息异常");
            }

            return AjaxResult.Success(corp.CorpID,"成功");
        }

        public async Task<List<string>> GetCompanyList()
        {
            //获取当前登录人所在公司
            var corp = CurrentUser.Corp;

            if (null == corp)
            {
                throw new MessageException("获取当前登录人信息异常");
            }

            var corpId = corp.CorpID;

            //管理账号具有所在公司
            if (CurrentUser.IsAdmin)
            {
                corpId = "80";
            }
            var sql = @"select corpid id,corp_sname text 
                          from (select c.corpid, cno, corp_sname, cname, corpparentid, '1' CLASSFLAG
                                  from cf_corp c
                                 where c.validflag = '1') t
                         start with t.corpid = @corpId
                        connect by prior corpid = corpparentid";

            var list = await _dbContext.SqlQueryAsync<ComboxData>(sql, new
            {
                corpId = corpId
            });

            List<string> returnList = new List<string>();

            foreach (var item in list)
            {
                returnList.Add(item.ID.ToString());
            }

            return returnList;
        }


        public async Task<List<ComboxData>> GetCompanyCombox()
        {
            //获取当前登录人所在公司
            var corp = CurrentUser.Corp;

            if (null == corp)
            {
                throw new MessageException("获取当前登录人信息异常");
            }

            var corpId = corp.CorpID;

            //管理账号具有所在公司
            if (CurrentUser.IsAdmin)
            {
                corpId = "80";
            }
            var sql = @"select corpid id,corp_sname text 
                          from (select c.corpid, cno, corp_sname, cname, corpparentid, '1' CLASSFLAG
                                  from cf_corp c
                                 where c.validflag = '1') t
                         start with t.corpid = @corpId
                        connect by prior corpid = corpparentid";

            var list = await _dbContext.SqlQueryAsync<ComboxData>(sql, new
            {
                corpId = corpId
            });

            return list;
        }

        public async Task<List<string>> GetCompanyListContainSpot()
        {
            //获取当前登录人所在公司
            var corp = CurrentUser.Corp;

            if (null == corp)
            {
                throw new MessageException("获取当前登录人信息异常");
            }

            var corpId = corp.CorpID;

            //管理账号具有所在公司
            if (CurrentUser.IsAdmin)
            {
                corpId = "80";
            }
            var sql = @"select corpid id,corp_sname text 
                          from (select c.corpid, cno, corp_sname, cname, corpparentid, '1' CLASSFLAG
                                  from cf_corp c
                                 where c.validflag = '1') t
                         start with t.corpid = @corpId
                        connect by prior corpid = corpparentid";

            var list = await _dbContext.SqlQueryAsync<ComboxData>(sql, new
            {
                corpId = corpId
            });

            List<string> returnList = new List<string>();

            foreach (var item in list)
            {
                returnList.Add("," + item.ID.ToString() + ",");
            }

            return returnList;
        }

        public async Task<List<string>> GetCompanyListContainSpot(string dept)
        {
            var corpId = dept;

            var sql = @"select corpid id,corp_sname text 
                          from (select c.corpid, cno, corp_sname, cname, corpparentid, '1' CLASSFLAG
                                  from cf_corp c
                                 where c.validflag = '1') t
                         start with t.corpid = @corpId
                        connect by prior corpid = corpparentid";

            var list = await _dbContext.SqlQueryAsync<ComboxData>(sql, new
            {
                corpId = corpId
            });

            List<string> returnList = new List<string>();

            foreach (var item in list)
            {
                returnList.Add("," + item.ID.ToString() + ",");
            }

            return returnList;
        }
    }
}
