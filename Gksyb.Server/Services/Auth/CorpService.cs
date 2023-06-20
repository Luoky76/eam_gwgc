using Gksyb.Core.Application;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Server.Interfaces.Auth;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 组织服务
    /// </summary>
    public partial class CorpService : BaseService<CF_CORP>, ICorpService
    {
        /// <summary>
        /// 组织服务
        /// </summary>
        public CorpService(IDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<CF_CORP>().OrderBy(c => c.CORPID).ToListAsync();
            var data = list.Select(c => new
            {
                ID = c.CORPID,
                TEXT = c.CORP_SNAME,
                PARENTID = (string.IsNullOrWhiteSpace(c.CORPPARENTID) || c.CORPPARENTID == "0") ? "ROOT" : c.CORPPARENTID,
                c.CLASSFLAG,
                ICON = "fa fa-group"
            }).ToList();
            data.Add(new { ID = "ROOT", TEXT = "组织结构", PARENTID = "", CLASSFLAG = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }

        /// <summary>
        /// 组织下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> CorpData()
        {
            return await _dbContext.Query<CF_CORP>()
                .Select(a => new ComboxData { ID = a.CORPID, TEXT = a.CORP_SNAME, VALUE = a.CORPID })
                .OrderBy(c => c.TEXT).ToListAsync();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<AjaxResult> SaveAsync(SaveRequest<CF_CORP> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.CNO, c.CORP_SNAME, c.CORP_ENAME, c.CNAME, c.CORPPARENTID, c.CORP_ADDRESS, c.CORP_TELE, c.CORP_FAX, c.CORP_EMAIL, c.CORP_LINK_MAN, c.LINK_MAN_TELE, c.LINK_MAN_EMAIL, c.FEECLIENT_ID, c.BANK, c.ACCONTNO, c.CWTELE, c.VALIDFLAG, c.CORP_PATH, c.CLASSFLAG, c.REMARK },
                c => a => a.CORPID == c.CORPID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(CF_CORP entity)
        {
            var isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CORP_SNAME == entity.CORP_SNAME).AnyAsync();
            if (isExists) throw new MessageException($"已经存在组织简称{entity.CORP_SNAME}");
            isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CNO == entity.CNO).AnyAsync();
            if (isExists) throw new MessageException($"已经存在组织代码{entity.CNO}");
            entity.CORPID = GuidHelper.NewShortId();
            var parentNode = "";
            if (!string.IsNullOrWhiteSpace(entity.CORPPARENTID))//有父节点
            {
                parentNode = await _dbContext.Query<CF_CORP>().Where(c => c.CORPID == entity.CORPPARENTID).Select(c => c.CORP_PATH).FirstOrDefaultAsync();
            }
            entity.CORP_PATH = await _dbContext.GetTreeNode("CF_CORP", parentNode, "", 3, "CORP_PATH");
            entity.RECORDSTATUS = Oper.Add;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(CF_CORP entity)
        {
            var model = await _dbContext.Query<CF_CORP>().Where(c => c.CORPID == entity.CORPID).FirstAsync();
            model.CORPPARENTID ??= "";
            if (entity.CORPPARENTID != model.CORPPARENTID)
            {
                var parentNode = "";
                if (!string.IsNullOrWhiteSpace(entity.CORPPARENTID))//有父节点
                {
                    parentNode = await _dbContext.Query<CF_CORP>().Where(c => c.CORPID == entity.CORPPARENTID).Select(c => c.CORP_PATH).FirstOrDefaultAsync();
                }
                entity.CORP_PATH = await _dbContext.GetTreeNode("CF_CORP", parentNode, "", 3, "CORP_PATH");
                if (model.CORP_PATH.HasValue())
                {
                    if (parentNode.StartsWith(model.CORP_PATH))
                    {
                        throw new MessageException("层级关系错误，上级不能直接改成下级");
                    }
                    var list = (await _dbContext.Query<CF_CORP>().Where(c => c.CORP_PATH.StartsWith(model.CORP_PATH) && c.CORPID != model.CORPID).ToListAsync()).OrderBy(c => c.CORP_PATH.Length);
                    foreach (var child in list)
                    {
                        var parent = list.FirstOrDefault(c => c.CORPID == child.CORPPARENTID) ?? (new CF_CORP() { CORP_PATH = entity.CORP_PATH });
                        var corpPath = await _dbContext.GetTreeNode("CF_CORP", parent.CORP_PATH, "", 3, "CORP_PATH");
                        await _dbContext.UpdateAsync<CF_CORP>(c => c.CORPID == child.CORPID, c => new CF_CORP()
                        {
                            CORP_PATH = corpPath
                        });
                    }
                }
            }
            entity.RECORDSTATUS = Oper.Modify;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(CF_CORP entity)
        {
            var isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CORPPARENTID == entity.CORPID).AnyAsync();
            if (isExists) throw new MessageException($"组织架构{entity.CORP_SNAME}存在子组织架构,无法删除");
            entity.RECORDSTATUS = Oper.Delete;
            entity.VALIDFLAG = "0";
        }
    }
}