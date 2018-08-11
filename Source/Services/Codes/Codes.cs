using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Codes : ICodes
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        public Result<object> AddScheme(SYS_Code_Scheme obj)
        {
            throw new NotImplementedException();
        }

        public Result<object> DeleteScheme(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> UpdateScheme(string id, SYS_Code_Scheme scheme)
        {
            throw new NotImplementedException();
        }

        public Result<object> EnableScheme(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetScheme(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetSchemes()
        {
            throw new NotImplementedException();
        }

        public Result<object> GetSerialRecord()
        {
            throw new NotImplementedException();
        }

        public Result<object> GetAllotRecord()
        {
            throw new NotImplementedException();
        }

        public Result<object> GetCodePreview(string id, string code, string mark)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据传入参数获取编码方案预览
        /// </summary>
        /// <param name="name">编码方案名称</param>
        /// <param name="id">业务记录ID</param>
        /// <param name="mark">标识符</param>
        /// <returns>Result</returns>
        public Result<object> GetCode(string name, string id, string mark)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.successful) return result;

            var bid = new GuidParse(id).Guid;
            if (!bid.HasValue)
            {
                result.InvalidGuid();
                return result;
            }

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

                var code = GetCode(scheme.ID, null, session.userId, bid.Value, null, mark);
                if (code == null) result.DataBaseError();
                else result.Success(code);

                return result;
            }
        }
    }
}
