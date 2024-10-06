using DummyClient;
using ServerCore;
using System;
internal class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPAcket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if( chatPAcket.playerId==1)
        Console.WriteLine(chatPAcket.chat);
    }
}