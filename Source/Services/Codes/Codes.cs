using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

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
            Guid bid;
            if (!Guid.TryParse(id, out bid)) return new JsonResult().InvalidGuid();

            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var session = verify.Session;
            using (var context = new BaseEntities())
            {
                var scheme = context.SYS_Code_Scheme.SingleOrDefault(s => s.Name == name);
                if (scheme == null)
                {
                    var ts = new ThreadStart(() => new Logger("001101", $"不存在名称为：【{name}】的编码方案", "编码规则", "生成编码").Write());
                    new Thread(ts).Start();
                    return result.CodeSchemeNotExists();
                }

                var code = GetCode(scheme.ID, null, session.UserId, bid, null, mark);
                return code == null ? result.DataBaseError() : result.Success(code.ToString());
            }
        }
    }
}
