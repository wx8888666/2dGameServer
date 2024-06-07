using _2DSurviveGameServer._01Common;
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
        Dictionary<long, Friends[]> uidFriendsList;
        Dictionary<Tuple<long, long>, RspChatMessage> ChatList;
        public override void Init()
        {
            base.Init();
            uidUserDic = new Dictionary<long, User>();
            uidHeartbeatDic = new Dictionary<long, long>();
            uidSessionDic = new Dictionary<long, ServerSession>();
            uidClientStateDic = new Dictionary<long, ClientStateEnum>();
            uidFriendsList = new Dictionary<long, Friends[]>();
            ChatList=new Dictionary<Tuple<long, long>, RspChatMessage>();
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
        public RspChatMessage GetChatList(long uid, long frienduid)
        {
            var key = CreateChatKey(uid, frienduid);
            ChatList.TryGetValue(key, out RspChatMessage chatlist);
            return chatlist;
        }
        public void AddChatList(long uid, long frienduid, RspChatMessage rspChatMessages)
        {
            var key = CreateChatKey(uid, frienduid);
            ChatList[key] = rspChatMessages;
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
        //添加好友系统
        public void UpdateFriendsList(long uid)
        {
            var friends = SqlSugarHelper.Db.Queryable<Friends>()
                             .Where(f => f.UId == uid)
                             .ToArray();

            if (uidFriendsList.ContainsKey(uid))
            {
                uidFriendsList[uid] = friends;
            }
            else
            {
                uidFriendsList.Add(uid, friends);
            }

        }
        public Friends[] GetFriends(long uId)
        {
            if (uidFriendsList.TryGetValue(uId, out var friends))
            {
                return friends;
            }

            return new Friends[0];
        }
        private Tuple<long, long> CreateChatKey(long uid, long frienduid)
        {
            // 使用排序来生成有序键值对
            return uid < frienduid ? Tuple.Create(uid, frienduid) : Tuple.Create(frienduid, uid);
        }

    }


}
