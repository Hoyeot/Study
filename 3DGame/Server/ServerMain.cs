using GameServerCS.Utils;
using System;
using System.Threading;

namespace GameServerCS
{
    public class ServerMain
    {
        /*
        GameServerCS/
        ├── Channel/            (채널 관련 클래스)
        │   ├── Channel.cs      (채널 기능 구현)
        │   ├── ChannelType.cs  (채널 타입 정의)
        ├── Client/             (클라이언트 클래스)
        │   ├── Client.cs       (클라이언트 로직)
        ├── Server/             (서버 클래스)
        │   ├── Server.cs       (서버 메인 로직)
        ├── Utils/              (공용 클래스)
        │   ├── Define.cs       (상수 정의)
        │   ├── Logger.cs       (로그파일 생성)
        ├── Program.cs          (프로그램 시작)
        └── ServerMain.cs       (서버 실행)
        */
        static void Main(string[] args)
        {
            try
            {
                Logger.Initialize();

                new Program();
                Logger.Log("서버 시작", "STARTUP");

                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("오류", ex);
            }
            finally
            {
                Logger.Log("서버 종료", "SHUTDOWN");
            }
        }
    }
}