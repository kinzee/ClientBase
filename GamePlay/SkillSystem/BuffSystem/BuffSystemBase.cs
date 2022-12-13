namespace GFramework.GamePlay
{
    public abstract class BuffSystemBase<T> : IBuffSystem, IRef where T : BuffDataBase
    {
        public BuffState BuffState { get; set; }
        public int CurrentOverlay { get; set; }
        public uint MaxDuration { get; set; }
        public BuffDataBase BuffData { get; set; }
        public long BuffNodeId { get; set; }
        public Unit UnitBelongto { get; set; }
        public Unit UnitFrom { get; set; }

        public T GetBuffData => BuffData as T;


        public void Init(BuffDataBase buffData, Unit unitFrom, Unit unitBelongto, float currentTime)
        {
            UnitFrom = unitFrom;
            UnitBelongto = unitBelongto;
            BuffData = buffData;

            // 加入成功
            if (true)
            {
                this.BuffState = BuffState.Waiting;
                OnInit();
            }
            else
            {
                RefPool.Release(this);
            }
        }

        public void Excute(float currentTime)
        {
            OnExecute();
        }

        public void Update(float currentTime)
        {
            OnUpdate();
        }

        public void Finished(float currentTime)
        {
            OnFinished();
        }

        public void Refresh(float currentTime)
        {
            OnRefreshed();
        }


        public virtual void OnInit()
        {
        }

        public virtual void OnExecute()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFinished()
        {
        }

        public virtual void OnRefreshed()
        {
        }

        public void Clear()
        {
            BuffState = BuffState.Waiting;
            CurrentOverlay = 0;
            MaxDuration = 0;
            BuffData = null;
            UnitFrom = null;
            UnitBelongto = null;
        }
    }
}