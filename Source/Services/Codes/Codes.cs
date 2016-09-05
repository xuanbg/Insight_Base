using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Codes : ICodes
    {
        public JsonResult AddScheme(SYS_Code_Scheme obj)
        {
            throw new NotImplementedException();
        }

        public JsonResult DeleteScheme(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult UpdateScheme(string id, SYS_Code_Scheme scheme)
        {
            throw new NotImplementedException();
        }

        public JsonResult EnableScheme(string id)
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
        /// <param name="mark">标识符</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetCode(string name, string id, string mark)
        {
            var result = new JsonResult();
            Guid bid;
            if (!Guid.TryParse(id, out bid))
            {
                result.InvalidGuid();
                return result;
            }

            var verify = new Compare(0);
            result = verify.Result;
            if (!result.Successful) return result;

            var session = verify.Basis;
            using (var context = new BaseEntities())
            {
                var scheme = context.SYS_Code_Scheme.SingleOrDefault(s => s.Name == name);
                if (scheme == null)
                {
                    var msg = $"不存在名称为：【{name}】的编码方案";
                    var ts = new ThreadStart(() => new Logger("001101", msg, "编码规则", "生成编码"));
                    new Thread(ts).Start();
                    result.CodeSchemeNotExists();
                    return result;
                }

                var code = GetCode(scheme.ID, null, session.UserId, bid, null, mark);
                if (code == null) result.DataBaseError();
                else result.Success(code);

                return result;
            }
        }
    }
}
