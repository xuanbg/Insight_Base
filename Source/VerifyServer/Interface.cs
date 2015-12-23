using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Insight.WS.Server
{
    [ServiceContract]
    interface Interface
    {

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <returns>全部内部用户的Session</returns>
        [OperationContract]
        List<Session> GetSessions();

        /// <summary>
        /// 获取用户Session
        /// </summary>
        /// <param name="obj">传入的用户Session</param>
        /// <returns>Session</returns>
        [OperationContract]
        Session GetSession(Session obj);

        /// <summary>
        /// 更新用户Session数据
        /// </summary>
        /// <param name="obj">传入的用户Session</param>
        [OperationContract]
        void UpdateSession(Session obj);

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="pw">密码MD5值</param>
        [OperationContract]
        bool UpdateSignature(int index, string pw);

        /// <summary>
        /// 重置指定用户Session的登录状态
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="status">在线状态</param>
        [OperationContract]
        bool SetOnlineStatus(int index, bool status);

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="validity">用户状态</param>
        [OperationContract]
        bool SetUserStatus(Guid uid, bool validity);

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>bool 是否成功</returns>
        [OperationContract]
        Session Verification(Session obj);

        /// <summary>
        /// 简单会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>bool 是否成功</returns>
        [OperationContract]
        bool SimpleVerifty(Session obj);

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>bool 是否成功</returns>
        [OperationContract]
        bool Authorization(Session obj, string action);

    }
}
