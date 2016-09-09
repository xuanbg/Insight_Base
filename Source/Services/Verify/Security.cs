using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using static Insight.Base.Common.Parameters;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Security : ISecurity
    {

        #region Verify

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
            var context = WebOperationContext.Current;
            if (context == null) return;

            var response = context.OutgoingResponse;
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetToken(string account, string signature, string deptid)
        {
            var token = new AccessToken {Account = account};

            return new Compare(token, signature, deptid).Result;
        }

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveToken()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            verify.Basis.SignOut();
            return result;
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult RefreshToken()
        {
            var verify = new Compare(60);
            return verify.Result;
        }

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult Verification()
        {
            return new Compare().Result;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult Authorization(string action)
        {
            return new Compare(action).Result;
        }

        #endregion

        #region SMSCode

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>JsonResult</returns>
        public JsonResult NewCode(string mobile, int type, int time)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var record = SmsCodes.OrderByDescending(r => r.CreateTime).FirstOrDefault(r => r.Mobile == mobile && r.Type == type);
            if (record != null && (DateTime.Now - record.CreateTime).TotalSeconds < 60)
            {
                result.TimeTooShort();
                return result;
            }

            var code = Parameters.Random.Next(100000, 999999).ToString();
            record = new VerifyRecord
            {
                Type = type,
                Mobile = mobile,
                Code = code,
                FailureTime = DateTime.Now.AddMinutes(time),
                CreateTime = DateTime.Now
            };
            SmsCodes.Add(record);

            var msg = $"已经为手机号【{mobile}】的用户生成了类型为【{type}】的短信验证码：【{code}】。此验证码将于{record.FailureTime}失效。";
            var ts = new ThreadStart(() => new Logger("700501", msg).Write());
            new Thread(ts).Start();

            result.Success(code);
            return result;
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>JsonResult</returns>
        public JsonResult VerifyCode(string mobile, string code, int type, bool remove = true)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null)
            {
                result.SMSCodeError();
            }

            if (!remove) return result;

            SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type);
            return result;
        }

        public JsonResult GetPicCode(string id)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            Guid vid;
            if (!Guid.TryParse(id, out vid)) 
            {
                result.InvalidGuid();
                return result;
            }

            var key = "";
            var pic = GenerateMark(image, index);
            var puzzle = new Puzzle
            {
                Name = picnum.ToString(),
                Fragments = new SortedDictionary<string, string>()
            };
            using (var ms = new MemoryStream())
            {
                pic.Save(ms, ImageFormat.Jpeg);
                puzzle.Cover = new byte[ms.Length];
                ms.Read(puzzle.Cover, 0, (int)ms.Length);
            }
            for (var i = 0; i < 9; i++)
            {
                var img = $"{picnum}_0{i + 1}.jpg";
                var fid = Guid.NewGuid().ToString().ToUpper();
                puzzle.Fragments.Add(fid, img);
                if (i == index) key = fid;
            }

            var record = new VerifyRecorrd { ID = vid, Key = key };
            VerifyImages.Add(record);
            var ts = new ThreadStart(() => new Logger("700502", $"图形验证记录ID：{id}；Key：{key}；Image：{picnum}.jpg；Fragment：{index}", "其他服务", "生成图形验证码"));
            new Thread(ts).Start();
            return result.Success(puzzle);
        }

        public JsonResult VerifyPicCode(string id, string code)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Session

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <param name="type">用户类型</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetSessions(string type)
        {
            const string action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var list = SessionManage.GetOnlineUsers(Convert.ToInt32(type));
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        #endregion

    }
}
