namespace SapphTools.DHA.Stig.Remediator.Extensions; 
public static class TimeSpanExtensions {
    public static string ToSmartString(this TimeSpan timeSpan, int fracDigits = 1, int layers = 1) {
        if (layers < 1) {
            throw new ArgumentOutOfRangeException(nameof(layers), "Layers of span scales must be greater than 0");
        }
        if (fracDigits < 1) {
            throw new ArgumentOutOfRangeException(nameof(fracDigits), "Number of significant figures must be greater than 0");
        }
        int currentLayer = 1;
        int layerCount = 0;
        List<string> layerStrings = [];
        long total = 0;
        while (layerCount < layers) {
            if (currentLayer > 6) {
                break;
            }
            int layerValue = timeSpan.GetValueAtLayer(currentLayer);
            long layerTicks = layerValue * GetTickValueAtLayer(currentLayer);
            if (layerValue == 0) {
                if ((total + layerTicks) != 0) {
                    layerCount++;
                }
                currentLayer++;
                continue;
            }
            layerCount++;
            if (layerCount == layers) {
                long remaining = timeSpan.Ticks - total;
                TimeSpan remainder = new(remaining);
                if (remainder.GetTotalValueStringAtLayer(currentLayer, fracDigits) is string thisLayer) {
                    layerStrings.Add(thisLayer);
                }
            } else {
                total += layerTicks;
                if (timeSpan.GetValueStringAtLayer(currentLayer) is string thisLayer) {
                    layerStrings.Add(thisLayer);
                }
            }
            currentLayer++;
        }
        if (layerStrings.Count == 0) {
            return "0 ticks";
        }
        return string.Join(", ", layerStrings);
    }
    public static string? GetValueStringAtLayer(this TimeSpan timeSpan, int layer) {
        if (layer < 1 || layer > 6) {
            throw new ArgumentOutOfRangeException(nameof(layer), "Available layers are 1 (Days) - 6 (Ticks) inclusive.");
        }
        int val = timeSpan.GetValueAtLayer(layer);
        return layer switch {
            1 => $"{val}d",
            2 => $"{val}h",
            3 => $"{val}m",
            4 => $"{val}s",
            5 => $"{val}ms",
            6 => $"{val} tick{((val == 0 || val > 1) ? "s" : "")}",
            _ => null,
        };
    }
    public static string? GetTotalValueStringAtLayer(this TimeSpan timeSpan, int layer, int fracDigits) {
        if (layer < 1 || layer > 6) {
            throw new ArgumentOutOfRangeException(nameof(layer), "Available layers are 1 (Days) - 6 (Ticks) inclusive.");
        }
        double val = Math.Round(timeSpan.GetTotalValueAtLayer(layer), fracDigits);
        return layer switch {
            1 => $"{val}d",
            2 => $"{val}h",
            3 => $"{val}m",
            4 => $"{val}s",
            5 => $"{val}ms",
            6 => $"{val} tick{((val == 0 || val > 1) ? "s" : "")}",
            _ => null,
        };
    }
    public static int GetValueAtLayer(this TimeSpan timeSpan, int layer) {
        if (layer < 1 || layer > 6) {
            throw new ArgumentOutOfRangeException(nameof(layer), "Available layers are 1 (Days) - 6 (Ticks) inclusive.");
        }
        return layer switch {
            1 => timeSpan.Days,
            2 => timeSpan.Hours,
            3 => timeSpan.Minutes,
            4 => timeSpan.Seconds,
            5 => timeSpan.Milliseconds,
            6 => timeSpan.ActualTicks(),
            _ => 0,
        };
    }
    public static double GetTotalValueAtLayer(this TimeSpan timeSpan, int layer) {
        if (layer < 1 || layer > 6) {
            throw new ArgumentOutOfRangeException(nameof(layer), "Available layers are 1 (Days) - 6 (Ticks) inclusive.");
        }
        return layer switch {
            1 => timeSpan.TotalDays,
            2 => timeSpan.TotalHours,
            3 => timeSpan.TotalMinutes,
            4 => timeSpan.TotalSeconds,
            5 => timeSpan.TotalMilliseconds,
            6 => timeSpan.Ticks,
            _ => 0,
        };
    }
    private static long GetTickValueAtLayer(int layer) {
        if (layer < 1 || layer > 6) {
            throw new ArgumentOutOfRangeException(nameof(layer), "Available layers are 1 (Days) - 6 (Ticks) inclusive.");
        }
        return layer switch {
            1 => TimeSpan.TicksPerDay,
            2 => TimeSpan.TicksPerHour,
            3 => TimeSpan.TicksPerMinute,
            4 => TimeSpan.TicksPerSecond,
            5 => TimeSpan.TicksPerMillisecond,
            6 => 1,
            _ => 0,
        };
    }
    public static int ActualTicks(this TimeSpan timeSpan) {
        long baseTicks = timeSpan.Ticks;
        long actualTicks = baseTicks - (timeSpan.Days * TimeSpan.TicksPerDay);
        actualTicks -= timeSpan.Hours * TimeSpan.TicksPerHour;
        actualTicks -= timeSpan.Minutes * TimeSpan.TicksPerMinute;
        actualTicks -= timeSpan.Seconds * TimeSpan.TicksPerSecond;
        actualTicks -= timeSpan.Milliseconds * TimeSpan.TicksPerMillisecond;
        return (int)actualTicks;
    }
}