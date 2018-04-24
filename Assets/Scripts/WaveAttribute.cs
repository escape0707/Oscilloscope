﻿/// <summary> 代表一条正弦波的参数组，由三个 float: a, omega, phi 组成 </summary>
internal class WaveAttribute {
    internal float a, omega, phi;
    internal WaveAttribute(float a = 1, float omega = 1, float phi = 0) {
        this.a = a;
        this.omega = omega;
        this.phi = phi;
    }
}