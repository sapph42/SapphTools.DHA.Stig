using SB = System.Text.StringBuilder;
using CH = System.Char;
using SR = System.IO.StreamReader;
using ST = System.String;
using IN = System.Int32;

#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE1006 // Naming Styles
namespace AbsoluteNonsense;
internal class S(ST input) {
    private readonly SR s = new(input);
    public void C() => s.Close();
    public void D() => s.Dispose();
    public IN P() => s.Peek();
    public IN R() => s.Read();
}
internal class B {
    private readonly SB b = new();
    public void A(ST input) => b.Append(input);
    public ST S() => b.ToString();
}
internal class C {
    private readonly CH c;
    private C(CH cha) {
        c = cha;
    }
    public static implicit operator CH(C ch) => ch.c;
    public static implicit operator C(char ch) => new(ch);
    public static implicit operator C(int i) => new((CH)i);
    public static implicit operator IN(C ch) => ch.c;
    public static ST operator *(C l, IN r) => new(l.c, r);
    public static IN operator -(C l, IN r) => l - r;
    public static C L(C ch) => new(CH.ToLowerInvariant(ch.c));
    public ST S() => c.ToString();
}
public static class U {
    private static S s() => new(@"C:\Users\n\OD\Launcher\input.txt");
    private static ST? t(S r) {
        if (r.P() == -1) return null;
        C c = C.L(r.R());
        return r.P() switch {
            >= 48 and <= 57 => p(c, r.R()),
            _ => l(c)
        };
    }
    private static ST l(C c) => c * (c - 96);
    private static ST p(C c, IN i) => c * i;
    public static ST i() {
        S r = U.s();
        B s = new();
        ST? a = ST.Empty;
        while (a is not null) {
            s.A(a);
            a = t(r);
        }
        r.D();
        return s.S();
    }
}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0130 // Namespace does not match folder structure