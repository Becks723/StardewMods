namespace JunimoStudio.Core
{
    /// <summary>拍号。</summary>
    public interface ITimeSignature
    {
        /// <summary>拍号的分子，表示一小节内的节拍数。</summary>
        int Numerator { get; set; }

        /// <summary>拍号的分母，表示一拍的相对长度。</summary>
        /// <remarks>比如，4表示以四分音符为一拍。</remarks>
        int Denominator { get; set; }
    }
}
