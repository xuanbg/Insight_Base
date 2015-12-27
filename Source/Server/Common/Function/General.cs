using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Insight.WS.Server.Common.Service;
using static Insight.WS.Server.Common.Util;

namespace Insight.WS.Server.Common
{
    public class General
    {
        #region 字段

        // 最低兼容版本
        private static readonly int CompatibleVersion = Convert.ToInt32(GetAppSetting("CompatibleVersion"));

        // 更新版本
        private static readonly int UpdateVersion = Convert.ToInt32(GetAppSetting("UpdateVersion"));

        // 访问验证服务用的Binding
        private static readonly Binding Binding = new NetTcpBinding();

        // 访问验证服务用的Address
        private static readonly EndpointAddress Address = new EndpointAddress(GetAppSetting("IvsAddress"));

        #endregion

        #region 接口验证

        /// <summary>
        /// 通过Session校验是否有权限访问
        /// </summary>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify()
        {
            var result = new JsonResult();
            var obj = GetAuthorization<Session>();
            if (obj == null) return result.InvalidAuth();

            if (obj.Version < CompatibleVersion || obj.Version > UpdateVersion) return result.Incompatible();

            var us = Verification(obj);
            switch (us.LoginResult)
            {
                case LoginResult.Success:
                    return result.Success();

                case LoginResult.Multiple:
                    return result.Multiple();

                case LoginResult.NotExist:
                    return result.Expired();

                case LoginResult.Failure:
                    return result.InvalidAuth();

                case LoginResult.Banned:
                    return result.Disabled();

                default:
                    return result;
            }
        }

        /// <summary>
        /// 通过指定的Rule校验是否有权限访问
        /// </summary>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify(string rule)
        {
            var result = new JsonResult();
            var obj = GetAuthorization<string>();
            return obj != rule ? result.InvalidAuth() : result.Success();
        }

        #endregion  

        #region 常用方法

        /// <summary>
        /// 构建用于接口返回值的Json对象
        /// </summary>
        /// <typeparam name="T">传入的对象类型</typeparam>
        /// <param name="obj">传入的对象</param>
        /// <returns>JsonResult</returns>
        public static JsonResult GetJson<T>(T obj)
        {
            var result = new JsonResult();
            return obj == null ? result.NotFound() : result.Success(Serialize(obj));
        }

        /// <summary>
        /// 保存图片到image文件夹，通过URL访问
        /// </summary>
        /// <param name="pic">图片字节流</param>
        /// <param name="catalog">分类</param>
        /// <param name="name">文件名（可选）</param>
        /// <returns>string 图片保存路径</returns>
        public static string SaveImage(byte[] pic, string catalog, string name = null)
        {
            var path = $"{GetAppSetting("ImageLocal")}\\{catalog}\\";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (name == null)
                {
                    var img = Image.FromStream(new MemoryStream(pic));
                    name = $"{Guid.NewGuid()}.{GetImageExtension(img)}";
                }

                File.WriteAllBytes(path + name, pic);
                return $"/{catalog}/{name}";
            }
            catch (Exception ex)
            {
                LogToEvent(ex.ToString());
                LogToEvent($"文件写入路径：{path}；字节流：{pic}");
                return null;
            }
        }

        #endregion

        #region 验证服务调用

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="type">验证类型</param>
        /// <param name="mobile">手机号</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>string 验证码</returns>
        public static string GetCode(int type, string mobile, int time = 30)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.NewCode(type, mobile, time);
            }
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="action">是否验证成功后删除记录</param>
        /// <returns>bool 是否正确</returns>
        public static bool VerifyCode(string mobile, string code, int type, bool action = true)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.VerifyCode(mobile, code, type, action);
            }
        }

        /// <summary>
        /// 获取验证服务器上的用户会话
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>Session 用户会话</returns>
        public static Session UserLogin(Session obj)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.UserLogin(obj);
            }
        }

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>全部内部用户的Session</returns>
        public static List<Session> GetSessions(Session obj)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.GetSessions(obj);
            }
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="id">用户ID</param>
        /// <param name="pw">新的密码</param>
        public static bool UpdateSignature(Session obj, Guid id, string pw)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.UpdateSignature(obj, id, pw);
            }
        }

        /// <summary>
        /// 根据用户ID更新Session用户信息
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="uid">用户ID</param>
        /// <returns>bool 是否成功</returns>
        public static bool UpdateUserInfo(Session obj, Guid uid)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.UpdateUserInfo(obj, uid);
            }
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="account">用户账号</param>
        public static bool SetUserOffline(Session obj, string account)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.SetUserOffline(obj, account);
            }
        }

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="uid">用户ID</param>
        /// <param name="validity">用户状态</param>
        /// <returns>bool 是否成功</returns>
        public static bool SetUserStatus(Session obj, Guid uid, bool validity)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.SetUserStatus(obj, uid, validity);
            }
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>bool 是否成功</returns>
        public static bool Verification(Session obj, string action)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.Authorization(obj, action);
            }
        }

        /// <summary>
        /// 会话合法性验证，用于持久化客户端
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>bool 是否成功</returns>
        public static Session Verification(Session obj)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.Verification(obj);
            }
        }

        /// <summary>
        /// 简单会话合法性验证，用于非持久化客户端
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>bool 是否成功</returns>
        public static bool SimpleVerifty(Session obj)
        {
            using (var client = new InterfaceClient(Binding, Address))
            {
                return client.SimpleVerification(obj);
            }
        }

        #endregion

    }
}
