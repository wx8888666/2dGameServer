using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol;
using Protocol.Body;
using Protocol.DBModel;
using System.Security.Cryptography;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._02Sys
{
    public class BagSys:SysRoot<BagSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqBag, ReqBag);
        }


        void ReqBag(MsgPack pack)
        {
            ReqBag req = pack.msg.reqBag;

            var existingBag = SqlSugarHelper.Db.Queryable<Bag>()
                                               .First(p => p.UId == req.UId);

            Bag newBag;
            if (existingBag == null)
            {
                // 如果数据库中不存在对应 UId 的 Bag 数据，创建一个新的 Bag 对象并插入数据库
                newBag = new Bag
                {
                    UId = req.UId,
                    GoldCoinsNum = 1,
                    MasonryNum = 1,
                };

                SqlSugarHelper.Db.Insertable(newBag).ExecuteCommand();
            }
            else
            {
                // 如果数据库中已存在对应 UId 的 Bag 数据，直接使用现有数据
                newBag = existingBag;
            }

            // 准备响应消息
            var rsp = new Protocol.DBModel.RspBag
            {
                bag = newBag
            };

            // 发送响应消息
            pack.session.SendMsg(new Msg
            {
                cmd = Protocol.CMD.RspBag,
                rspBag = rsp
            });
        }

    }


}
