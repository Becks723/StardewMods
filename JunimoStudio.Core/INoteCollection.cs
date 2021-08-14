using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    public interface INoteCollection : IEnumerable<INote>, INotifyCollectionChanged
    {
        /// <summary>
        /// 向该<see cref="INoteCollection"/>实例中添加一个<see cref="INote"/>。
        /// </summary>
        /// <param name="note">要添加的<see cref="INote"/>。不可为<see langword="null"/>.</param>
        void Add(INote note);

        /// <summary>
        /// 向该<see cref="INoteCollection"/>实例中添加一个符合指定条件的<see cref="INote"/>，并作为返回值返回。
        /// </summary>
        /// <param name="pitch">指定该音符音高，见<seealso cref="INote.Number"/>。</param>
        /// <param name="start">指定该音符开始时间，见<seealso cref="INote.Start"/>。</param>
        /// <param name="duration">指定该音符时长，见<seealso cref="INote.Duration"/>。</param>
        /// <param name="notify">指定该音符是否具备通知能力，即，是否继承自<see cref="INotifyPropertyChanged"/>接口.</param>
        /// <returns>新添加的<see cref="INote"/>。</returns>
        INote Add(int pitch, long start, int duration);

        /// <summary>
        /// 从该<see cref="INoteCollection"/>实例中移除一个<see cref="INote"/>。
        /// </summary>
        /// <param name="note">要移除的<see cref="INote"/>。</param>
        /// <returns>是否成功移除。</returns>
        bool Remove(INote note);
    }
}
