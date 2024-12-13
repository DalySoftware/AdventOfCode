using System.Runtime.CompilerServices;

namespace Day13;

class Tests
{
    class TestFailException([CallerMemberName] string caller = "") : Exception("A test failed: " + caller);

    internal static void Run()
    {
        // FindsRootWithoutExtraCondition();
        // FindsRootWithoutExtraCondition2();
    }


    // internal static void FindsRootWithoutExtraCondition()
    // {
    //     var strategies = Extensions.CandidateStrategies(7, 11, 77, _ => true).ToArray();
    //
    //     if (!strategies.Any(s => s.APresses == 11 || s.BPresses == 0))
    //         throw new TestFailException();
    //
    //     if (!strategies.Any(s => s.APresses == 0 || s.BPresses == 7))
    //         throw new TestFailException();
    // }
    //
    // internal static void FindsRootWithoutExtraCondition2()
    // {
    //     var strategies = Extensions.CandidateStrategies(21, 14, 98, _ => true).ToArray();
    //
    //     if (!strategies.Any(s => s.APresses == 2 || s.BPresses == 4))
    //         throw new TestFailException();
    //
    //     if (!strategies.Any(s => s.APresses == 5 || s.BPresses == 1))
    //         throw new TestFailException();
    // }
}