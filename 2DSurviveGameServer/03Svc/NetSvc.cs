using _2DSurviveGameServer._01Common;
using PENet;
using PEUtils;
using Protocol;

namespace _2DSurviveGameServer._03Svc
{
    /// <summary>
    /// 服务端的消息包体（因为服务端需要知道是哪个客户端发来的消息，所以需要Session一起结合消息包）
    /// </summary>
    public class MsgPack
    {
        public ServerSession session;//连接的客户端
        public Msg msg;//消息包

        public MsgPack(ServerSession session,Msg msg)
        {
            this.session = session;
            this.msg = msg;
        }
    }

    public class NetSvc:SvcRoot<NetSvc>
    {
        string ip = "0.0.0.0";//服务端监听的ip，0.0.0.0是代表监听所有本机IP地址（如：内网IP、外网IP）
        int port = 18888;//服务端监听的端口号，一般是0到65535
        public static readonly string pkgque_lock = "pkgque_lock";//消息队列锁
        KCPNet<ServerSession, Msg> server;//服务端，与客户端的类似
        Queue<MsgPack> msgPackQueue;//消息队列
        Dictionary<CMD, Action<MsgPack>> msgHandleDic;//消息处理事件字典

        public override void Init()
        {
            base.Init();
            server = new KCPNet<ServerSession, Msg>();
            msgPackQueue = new Queue<MsgPack>();
            msgHandleDic = new Dictionary<CMD, Action<MsgPack>>();
            //设置输出日志
            KCPTool.LogFunc = this.Log;
            KCPTool.WarnFunc = this.Warn;
            KCPTool.ErrorFunc = this.Error;
            KCPTool.ColorLogFunc = (c, s) => { this.ColorLog((LogColor)c, s); };

            server.StartAsServer(ip, port);//启动服务端



        }


        public override void Update()
        {
            base.Update();

            //如果消息队列里有消息则进行处理（注意：这里为什么在Update里面还要使用while来处理呢？为什么客户端那边同样在Update里面而不使用while来处理呢？因为服务端需要处理很多客户端的消息，并且每个客户端有可能会发大量的消息过来，服务端就必须要能及时处理掉，否则就会出现客户端那边延迟高的现象，而客户端不用while的原因是因为客户端只用处理自己跟服务端的消息，一般来说消息量是很少的，直接使用update来驱动处理就ok）
            while (msgPackQueue.Count > 0)
            {
                lock (pkgque_lock)
                {
                    //从消息队列中取出一个消息包
                    MsgPack pack = msgPackQueue.Dequeue();
                    //进行处理
                    HandleMsg(pack);
                }
            }
        }

        /// <summary>
        /// 添加消息到消息队列
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        public void AddMsgQueue(ServerSession session,Msg msg)
        {
            lock (pkgque_lock)
            {
                msgPackQueue.Enqueue(new MsgPack(session, msg));
                //Monitor.Pulse(pkgque_lock);
            }
        }

        /// <summary>
        /// 添加消息处理方法
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="handle"></param>
        public void AddMsgHandle(CMD cmd,Action<MsgPack> handle)
        {
            msgHandleDic.Add(cmd, handle);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="pack"></param>
        void HandleMsg(MsgPack pack)
        {
            //这里通过观察者，策略设计模式来唤醒对应的处理逻辑
            if(msgHandleDic.TryGetValue(pack.msg.cmd,out Action<MsgPack> handle))
            {
                handle.Invoke(pack);
            }
            else
            {
                //没有找到对应处理方法
                this.Error("Not Found Handle "+pack.msg.cmd);
            }
        }
        //这里没有注销这个委托的原因是msgHandleDic是NetSvc类的成员，并且它的生命周期与NetSvc实例的生命周期一致。
        //当NetSvc实例被回收时，msgHandleDic以及其中包含的所有委托也会被回收。而且整体项目不大。
    }
}
