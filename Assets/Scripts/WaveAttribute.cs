/// <summary> 代表一条正弦波的参数组，由 三个float：a， omega， phi 组成 </summary>
internal class WaveAttribute {
    internal float A, Omega, Phi;

    /// <summary> 初始化构造函数 </summary>
    internal WaveAttribute(float a = 1, float omega = 1, float phi = 0) {
        this.A = a;
        this.Omega = omega;
        this.Phi = phi;
    }
}

/// <summary> 代表波的一次修改的参数组，由 三个float：a， omega， phi 组成 </summary>
internal class WaveModification : WaveAttribute {
    /// <summary> 初始化构造函数 </summary>
    internal WaveModification(float a = 1, float omega = 1, float phi = 0):
        base(a, omega, phi) { }

    /// <summary> 拷贝构造 </summary>
    internal WaveModification(WaveModification other):
        this(other.A, other.Omega, other.Phi) { }

    /// <summary>
    /// 叠加另一个 WaveModification 效果
    /// </summary>
    /// <param name="other"> 要叠加的 WaveModification </param>
    internal void StageWith(WaveModification other) {
        A *= other.A;
        Omega *= other.Omega;
        Phi += other.Phi;
    }
}