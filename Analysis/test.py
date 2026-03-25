from math import isclose

def exact_integer_pairs(B, pop_min=2, pop_max=None):
    """
    Return all exact integer (Pop, Gen) pairs such that Pop * Gen == B.
    """
    if pop_max is None:
        pop_max = B

    pairs = []
    for pop in range(pop_min, pop_max + 1):
        if B % pop == 0:
            gen = B // pop
            pairs.append((pop, gen))
    return pairs


def evenly_spaced_exact_pairs(B, k=None, pop_min=2, pop_max=None):
    """
    Return exact integer pairs, optionally reduced to k pairs spread as evenly
    as possible over the Pop axis.
    """
    pairs = exact_integer_pairs(B, pop_min, pop_max)

    if k is None or k >= len(pairs):
        return pairs

    # Pick k approximately evenly distributed indices
    idxs = [round(i * (len(pairs) - 1) / (k - 1)) for i in range(k)]
    idxs = sorted(set(idxs))

    # If rounding caused duplicates, fill missing spots greedily
    while len(idxs) < k:
        for j in range(len(pairs)):
            if j not in idxs:
                idxs.append(j)
                if len(idxs) == k:
                    break
        idxs.sort()

    return [pairs[i] for i in idxs]


def best_linear_approx_pairs(B, k, pop_start, pop_end):
    """
    Generate k integer pairs with Pop roughly linear:
        P_i ~= P_0 + e*i
    and Gen chosen so that Pop*Gen stays as close as possible to B.

    Then estimate average step sizes:
        e ~= avg(P_{i+1} - P_i)
        f ~= avg(G_i - G_{i+1})

    Returns:
        pairs, e_est, f_est, total_budget_error
    """
    if k < 2:
        raise ValueError("k must be at least 2")

    e = (pop_end - pop_start) / (k - 1)
    pops = [round(pop_start + e * i) for i in range(k)]

    # Remove duplicates if rounding collapses values
    pops = sorted(set(pops))
    if len(pops) < 2:
        raise ValueError("Pop range too small after rounding")

    pairs = []
    total_error = 0
    for p in pops:
        g = max(1, round(B / p))
        pairs.append((p, g))
        total_error += abs(B - p * g)

    pop_steps = [pairs[i+1][0] - pairs[i][0] for i in range(len(pairs)-1)]
    gen_steps = [pairs[i][1] - pairs[i+1][1] for i in range(len(pairs)-1)]

    e_est = sum(pop_steps) / len(pop_steps)
    f_est = sum(gen_steps) / len(gen_steps)

    return pairs, e_est, f_est, total_error


def best_e_close_f_exact(B, pop_min=2, pop_max=None):
    """
    Among exact integer solutions Pop*Gen=B, find the longest contiguous subsequence
    of divisor pairs for which average Pop step e is closest to average Gen step f.
    This is useful if you want 'e ~= f' while keeping exact integer solutions.

    Returns:
        best_subsequence, e_est, f_est, abs(e_est - f_est)
    """
    pairs = exact_integer_pairs(B, pop_min, pop_max)
    if len(pairs) < 2:
        return pairs, 0, 0, 0

    best = None

    for a in range(len(pairs)):
        for b in range(a + 1, len(pairs)):
            sub = pairs[a:b+1]
            if len(sub) < 2:
                continue

            pop_steps = [sub[i+1][0] - sub[i][0] for i in range(len(sub)-1)]
            gen_steps = [sub[i][1] - sub[i+1][1] for i in range(len(sub)-1)]

            e_est = sum(pop_steps) / len(pop_steps)
            f_est = sum(gen_steps) / len(gen_steps)
            diff = abs(e_est - f_est)

            score = (diff, -len(sub))  # prefer smaller diff, then longer subsequence
            if best is None or score < best[0]:
                best = (score, sub, e_est, f_est, diff)

    _, sub, e_est, f_est, diff = best
    return sub, e_est, f_est, diff


if __name__ == "__main__":
    B = 40000

    print("=== All exact integer pairs ===")
    pairs = exact_integer_pairs(B, pop_min=2, pop_max=256)
    print(pairs)
    print("total pairs =", len(pairs))

    print("\n=== 8 evenly distributed exact pairs ===")
    e_pairs= evenly_spaced_exact_pairs(B, k=10, pop_min=2, pop_max=200)
    print(e_pairs)
    print("total pairs =", len(e_pairs))

    # print("\n=== Best linear-ish approximation ===")
    # approx_pairs, e_est, f_est, err = best_linear_approx_pairs(
    #     B=B,
    #     k=10,
    #     pop_start=20,
    #     pop_end=200,
    # )
    # print("pairs =", approx_pairs)
    # print("e ~= ", e_est)
    # print("f ~= ", f_est)
    # print("total budget error =", err)
    # print("total pairs =", len(approx_pairs))

    print("\n=== Exact subsequence where e ~= f most closely ===")
    sub, e_est, f_est, diff = best_e_close_f_exact(B, pop_min=2, pop_max=250)
    print("pairs =", sub)
    print("e ~= ", e_est)
    print("f ~= ", f_est)
    print("|e-f| =", diff)
    print("total pairs =", len(sub))
    