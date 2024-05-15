﻿using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;

namespace _2DSurviveGameServer._03Svc
{
    public class CacheSvc:SvcRoot<CacheSvc>
    {
        Dictionary<long, User> uidUserDic;//玩家UID对应的玩家信息
        Dictionary<long, long> uidHeartbeatDic;//玩家UID对应心跳时间
        Dictionary<long, ServerSession> uidSessionDic;//玩家UID对应连接SID
        Dictionary<long, ClientStateEnum> uidClientStateDic;

        public override void Init()
        {
            base.Init();
            uidUserDic = new Dictionary<long, User>();
            uidHeartbeatDic = new Dictionary<long, long>();
            uidSessionDic = new Dictionary<long, ServerSession>();
            uidClientStateDic = new Dictionary<long, ClientStateEnum>();
        }



        /// <summary>
        /// 更新保存玩家UID对应玩家信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="user"></param>
        public void UpdateUidUser(long uid,User user)
        {
            uidUserDic[uid] = user;
        }

        public void RemoveUser(long uid)
        {
            uidUserDic.Remove(uid);
            uidHeartbeatDic.Remove(uid);
            uidSessionDic.Remove(uid);
        }

        /// <summary>
        /// 获取对应玩家UID的玩家信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public User GetUser(long uid)
        {
            if(uidUserDic.ContainsKey(uid))
            {
                return uidUserDic[uid];
            }

            return null;
        }

        /// <summary>
        /// 更新对应玩家UID的心跳时间
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="session"></param>
        public void UpdateUserHeartbeat(long uid,ServerSession session)
        {
            uidHeartbeatDic[uid] = TimeHelper.GetUTCStartMilliseconds();
            if(uid != 0)
            {
                if(uidSessionDic.TryGetValue(uid,out ServerSession getSession))
                {
                    if(getSession != session)
                    {
                        this.Log($"uid:{uid} 变更连接 sid:{session.GetSessionID()}");
                        uidSessionDic[uid] = session;
                    }
                }
                else
                {
                    uidSessionDic[uid] = session;
                }
            }
        }

        /// <summary>
        /// 判定对应玩家是否在线
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsOnline(long uid)
        {
            if(uidHeartbeatDic.TryGetValue(uid,out long time))
            {
                if(TimeHelper.GetUTCStartMilliseconds() - time > 10000)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }


        public ServerSession GetSession(long uid)
        {
            uidSessionDic.TryGetValue(uid, out ServerSession session);
            return session;
        }

        public void UpdateClientState(long uid,ClientStateEnum stateEnum)
        {
            uidClientStateDic[uid] = stateEnum;
        }

        public ClientStateEnum GetClientState(long uid)
        {
            uidClientStateDic.TryGetValue(uid, out var clientState);
            return clientState;
        }

    }

    
}
