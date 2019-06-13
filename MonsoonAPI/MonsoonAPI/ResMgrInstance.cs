namespace MonsoonAPI {
    public class ResMgrInstance {
        static IMonsoonResMgr _instance;

        public static IMonsoonResMgr GetResMgrInstance()
        {
            if (null == _instance)
            {
                _instance = new ResMgr();
            }
            return _instance;
        }
    }
}
