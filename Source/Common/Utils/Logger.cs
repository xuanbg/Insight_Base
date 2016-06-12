using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Utils
{
    public class Logger
    {
        /// <summary>
        /// 日志服务器返回结果
        /// </summary>
        public JsonResult Result;

        /// <summary>
        /// 事件代码
        /// </summary>
        private readonly string Code;

        /// <summary>
        /// 事件消息
        /// </summary>
        private readonly string Message;

        /// <summary>
        /// 事件源
        /// </summary>
        private readonly string Source;

        /// <summary>
        /// 事件名称
        /// </summary>
        private readonly string Action;

        /// <summary>
        /// 查询关键字
        /// </summary>
        private readonly string Key;

        /// <summary>
        /// 事件源用户ID
        /// </summary>
        private readonly Guid? UserId;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        /// <param name="source">事件来源</param>
        /// <param name="action">操作名称</param>
        /// <param name="key">查询关键字</param>
        /// <param name="uid">事件源用户ID</param>
        /// <returns>bool 是否写入成功</returns>
        public Logger(string code, string message = null, string source = null, string action = null, string key = null, Guid? uid = null)
        {
            Code = code;
            Message = message;
            Source = source;
            Action = action;
            Key = key;
            UserId = uid;
        }

        /// <summary>
        /// 将事件消息写到日志服务器
        /// </summary>
        public void Write()
        {
            var url = Util.LogServer + "logs";
            var dict = new Dictionary<string, object>
            {
                {"code", Code},
                {"message", Message},
                {"source", Source},
                {"action", Action},
                {"key", Key},
                {"userid", UserId}
            };
            var data = Util.Serialize(dict);
            var author = Util.Base64(Util.Hash(Code + Util.Secret));
            Result = new HttpRequest(url, "POST", author, data).Result;
        }
    }
}
