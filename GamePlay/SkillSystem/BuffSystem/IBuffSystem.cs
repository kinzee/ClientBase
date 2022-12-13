
namespace GFramework.GamePlay
{
    public interface IBuffSystem
    {
        /// <summary>
        /// Buff当前状态
        /// </summary>
        BuffState BuffState { get; set; }
        
        /// <summary>
        /// 当前叠加数
        /// </summary>
        int CurrentOverlay { get; set; }
        
        /// <summary>
        /// 持续时间，到达时间后移除
        /// </summary>
        uint MaxDuration { get; set; }
        
        /// <summary>
        /// Buff数据
        /// </summary>
        BuffDataBase BuffData { get; set; }
        
        /// <summary>
        /// Buff节点Id
        /// </summary>
        long BuffNodeId { get; set; }
        
        /// <summary>
        /// 来自哪个Unit
        /// </summary>
        Unit UnitFrom { get; set; }
        
        Unit UnitBelongto { get; set; }

        void Init(BuffDataBase buffData, Unit unitFrom, Unit unitBelongto, float currentTime);

        void Excute(float currentTime);

        void Update(float currentTime);

        void Finished(float currentTime);

        void Refresh(float currentTime);

    }
}