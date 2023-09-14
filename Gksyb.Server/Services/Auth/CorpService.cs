using Chloe.Annotations;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.Tree;
using Gksyb.Model.UI;
using Gksyb.Server.Interfaces.Auth;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 组织服务
    /// </summary>
    public partial class CorpService : BaseService<CF_CORP>, ICorpService
    {
        private readonly UserSession _user;
        /// <summary>
        /// 组织服务
        /// </summary>
        public CorpService(IDbContext dbContext, UserSession userSession) : base(dbContext)
        {
            _user = userSession;
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<CF_CORP>().OrderBy(c => c.CORPID).ToListAsync();
            var data = list.Select(c => new
            {
                ID = c.CORPID,
                TEXT = c.CORP_SNAME,
                PARENTID = (string.IsNullOrWhiteSpace(c.CORPPARENTID) || c.CORPPARENTID == "0") ? "ROOT" : c.CORPPARENTID,
                c.CORP_PATH,
                c.CLASSFLAG,
                ICON = "fa fa-group"
            }).ToList();
            data.Add(new { ID = "ROOT", TEXT = "组织结构", PARENTID = "", CORP_PATH = "", CLASSFLAG = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 组织下拉
        /// </summary>
        public async Task<List<ComboxData>> CorpData()
        {
            return await _dbContext.Query<CF_CORP>()
                .Select(a => new ComboxData { ID = a.CORPID, TEXT = a.CORP_SNAME, VALUE = a.CORPID })
                .OrderBy(c => c.TEXT).ToListAsync();
        }

        /// <summary>
        /// 保存
        /// </summary>
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
        private async Task BeforeAdd(CF_CORP entity)
        {
            await Handle(entity);
            entity.CORPID = GuidHelper.NewShortId();
            entity.CORP_PATH = await _dbContext.TreeHandle(CF_CORP_TREE.From(entity), null);
            entity.RECORDSTATUS = Oper.Add;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        private async Task BeforeUpdate(CF_CORP entity)
        {
            await Handle(entity);
            var model = await _dbContext.Query<CF_CORP>().Where(c => c.CORPID == entity.CORPID).FirstAsync();
            model.CORPPARENTID ??= "";
            if (entity.CORPPARENTID != model.CORPPARENTID)
            {
                entity.CORP_PATH = await _dbContext.TreeHandle(CF_CORP_TREE.From(entity), model.CORP_PATH);
            }
            entity.RECORDSTATUS = Oper.Modify;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        private async Task BeforeDelete(CF_CORP entity)
        {
            var isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CORPPARENTID == entity.CORPID).AnyAsync();
            if (isExists) throw new MessageException($"组织架构{entity.CORP_SNAME}存在子组织架构,无法删除");
            entity.RECORDSTATUS = Oper.Delete;
            entity.VALIDFLAG = "0";
        }

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task Handle(CF_CORP entity)
        {
            var isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CORP_SNAME == entity.CORP_SNAME)
                .WhereIfNotNullOrEmpty(entity.CORPID, c => c.CORPID != entity.CORPID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在组织简称{entity.CORP_SNAME}");
            isExists = await _dbContext.Query<CF_CORP>().Where(c => c.CNO == entity.CNO)
                .WhereIfNotNullOrEmpty(entity.CORPID, c => c.CORPID != entity.CORPID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在组织代码{entity.CNO}");
        }
    }

    [Table("CF_CORP")]
    public class CF_CORP_TREE : ITreeable
    {
        [Column("CORPID")]
        public string ID { get; set; }

        [Column("CORPPARENTID")]
        public string PARENTID { get; set; }

        [Column("CORP_PATH")]
        public string TREENODE { get; set; }

        public static CF_CORP_TREE From(CF_CORP corp) => new()
        {
            ID = corp.CORPID,
            PARENTID = corp.CORPPARENTID,
            TREENODE = corp.CORP_PATH
        };
    }
}