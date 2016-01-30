using System;
using System.Linq;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    public partial class BaseService : ICodes
    {
        public JsonResult AddScheme(SYS_Code_Scheme obj)
        {
            throw new NotImplementedException();
        }

        public JsonResult DeleteScheme(Guid id)
        {
            throw new NotImplementedException();
        }

        public JsonResult UpdateScheme(SYS_Code_Scheme obj)
        {
            throw new NotImplementedException();
        }

        public JsonResult EnableScheme(Guid id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetScheme(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetSchemes()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetSerialRecord()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetAllotRecord()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetCodePreview(string id, string code, string mark)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据传入参数获取编码方案预览
        /// </summary>
        /// <param name="name">编码方案名称</param>
        /// <param name="id">业务记录ID</param>
        /// <param name="mark">标记符</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetCode(string name, string id, string mark)
        {
            var verify = new Verify();
            if (!verify.Compare()) return verify.Result;

            Guid oid;
            if (!Guid.TryParse(id, out oid)) return verify.Result.InvalidGuid();

            var session = verify.Session;
            using (var context = new BaseEntities())
            {
                var scheme = context.SYS_Code_Scheme.SingleOrDefault(s => s.Name == name);
                if (scheme == null)
                {
                    General.LogToLogServer("001101", $"不存在名称为：【{name}】的编码方案", "编码规则", "生成编码");
                    return verify.Result.ServiceUnavailable();
                }

                var code = context.GetCode(scheme.ID, null, session.UserId, oid, null, mark);
                return code == null ? verify.Result.DataBaseError() : verify.Result.Success(code.ToString());
            }
        }
    }
}
