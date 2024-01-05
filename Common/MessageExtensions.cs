using QuickFix;
using QuickFix.Fields;
using System.Globalization;
using System.Text;

namespace Common
{
    public static class MessageExtensions
    {
        public static string GetMessageText<TMessage>(this TMessage message) where TMessage : Message
        {
            var properties = message.GetType().GetProperties();

            var ignoredPropertyNames = new string[] { "Header", "Trailer", "RepeatedTags", "FieldOrder" };

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Message: ");

            stringBuilder.AppendLine("{");

            foreach (var property in properties)
            {
                if (property.CanRead is false || ignoredPropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase)) continue;

                try
                {
                    stringBuilder.AppendLine($"    {property.Name}: \"{property.GetValue(message)}\",");
                }
                catch (ApplicationException)
                {
                }
            }

            stringBuilder.AppendLine("    All Fields: ");
            stringBuilder.AppendLine("    [");

            var fields = message.ToString().Split('').Where(field => string.IsNullOrWhiteSpace(field) is false).ToArray();

            var lastField = fields.Last();

            foreach (var field in fields)
            {
                var tagValue = field.Split('=');

                if (tagValue.Length < 2) continue;

                var comma = field.Equals(lastField, StringComparison.OrdinalIgnoreCase) ? "" : ",";

                stringBuilder.AppendLine($"        {{{tagValue[0]}: \"{tagValue[1]}\"}}{comma}");
            }

            stringBuilder.AppendLine("    ],");
            stringBuilder.AppendLine($"    Raw: \"{message.ToString('|')}\"");

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public static string ToString<TMessage>(this TMessage message, char separator) where TMessage : Message
        {
            try
            {
                return message.ToString().Replace('', separator);
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        public static IEnumerable<Symbol> GetSymbols(this QuickFix.FIX44.SecurityList message)
        {
            var numberOfGroups = message.GetInt(Tags.NoRelatedSym);

            var symbolField = new IntField(Tags.Symbol);
            var symbolNameField = new StringField(Tags.SideReasonCd);
            var symbolDigitsField = new IntField(Tags.SideTrdSubTyp);

            for (int groupIndex = 1; groupIndex <= numberOfGroups; groupIndex += 1)
            {
                var group = message.GetGroup(groupIndex, Tags.NoRelatedSym);

                var symbolFieldValue = group.GetField(symbolField).getValue();
                var symbolNameValue = group.GetField(symbolNameField).getValue();
                var symbolDigitsFieldValue = group.GetField(symbolDigitsField).getValue();

                yield return new Symbol(symbolFieldValue, symbolNameValue, symbolDigitsFieldValue);
            }
        }

        public static SymbolQuote GetSymbolQuote(this QuickFix.FIX44.MarketDataSnapshotFullRefresh message)
        {
            var numberOfGroups = message.GetInt(Tags.NoMDEntries);

            decimal bid = 0;
            decimal ask = 0;

            var mdEntryTypeField = new CharField(Tags.MDEntryType);
            var mdEntryPxField = new DecimalField(Tags.MDEntryPx);

            for (int groupIndex = 1; groupIndex <= numberOfGroups; groupIndex += 1)
            {
                var group = message.GetGroup(groupIndex, Tags.NoMDEntries);

                var mdEntryTypeFieldValue = group.GetField(mdEntryTypeField).getValue();
                var mdEntryPxFieldValue = group.GetField(mdEntryPxField).getValue();

                if (mdEntryTypeFieldValue == '0')
                {
                    bid = mdEntryPxFieldValue;
                }
                else if (mdEntryTypeFieldValue == '1')
                {
                    ask = mdEntryPxFieldValue;
                }
            }

            return new SymbolQuote(message.GetField(new IntField(Tags.Symbol)).getValue(),"", bid, ask, 0);
        }

        public static Position GetPosition(this QuickFix.FIX44.PositionReport message)
        {
            var noPositionsGroup = message.GetGroup(1, Tags.NoPositions);

            var longVolume = noPositionsGroup.GetDecimal(704);
            var shortVolume = noPositionsGroup.GetDecimal(705);

            decimal volume;
            string tradeSide;

            if (longVolume > shortVolume)
            {
                volume = longVolume;
                tradeSide = "Buy";
            }
            else
            {
                volume = shortVolume;
                tradeSide = "Sell";
            }

            return new Position
            {
                Id = long.Parse(message.PosMaintRptID.getValue(), NumberStyles.Any, CultureInfo.InvariantCulture),
                SymbolId = int.Parse(message.Symbol.getValue(), NumberStyles.Any, CultureInfo.InvariantCulture),
                EntryPrice = message.SettlPrice.getValue(),
                Volume = volume,
                TradeSide = tradeSide,
                StopLoss = message.IsSetField(1002) ? message.GetDecimal(1002) : 0,
                TakeProfit = message.IsSetField(1000) ? message.GetDecimal(1000) : 0,
                TrailingStopLoss = message.IsSetField(1004) ? message.GetBoolean(1004) : null,
                GuaranteedStopLoss = message.IsSetField(1006) ? message.GetBoolean(1006) : null,
                StopLossTriggerMethod = message.IsSetField(1005) ? message.GetInt(1005) switch
                {
                    1 => "Trade Side",
                    2 => "Opposite Side",
                    3 => "Double Trade Side",
                    4 => "Double Opposite Side",
                    _ => string.Empty
                } : string.Empty
            };
        }

        public static Order GetOrder(this QuickFix.FIX44.ExecutionReport message)
        {
            decimal targetPrice = 0;

            if (message.IsSetField(44))
            {
                targetPrice = message.Price.getValue();
            }
            else if (message.IsSetField(99))
            {
                targetPrice = message.StopPx.getValue();
            }

            return new Order
            {
                Id = long.Parse(message.OrderID.getValue(), NumberStyles.Any, CultureInfo.InvariantCulture),
                ClOrderId = message.ClOrdID.getValue(),
                PositionId = message.IsSetField(721) ? message.GetString(721) :string.Empty,
                OrderStatus = message.OrdStatus.getValue().ToString(),
                Type = message.GetInt(40) switch
                {
                    1 => "Market",
                    2 => "Limit",
                    3 => "Stop",
                    _ => string.Empty
                },
                SymbolId = int.Parse(message.Symbol.getValue(), NumberStyles.Any, CultureInfo.InvariantCulture),
                TargetPrice = targetPrice,
                Volume = message.OrderQty.getValue(),
                LeavesQty = message.IsSetField(151) ? message.LeavesQty.getValue() : 0,
                CumQty = message.IsSetField(14) ? message.CumQty.getValue() : 0,
                TimeInForce = message.IsSetField(59) ? message.GetInt(59) switch
                {
                    0 => "Day",
                    1 => "Good Till Cancel (GTC)",
                    2 => "At the Opening (OPG)",
                    3 => "Immediate or Cancel (IOC)",
                    4 => "Fill or Kill (FOK)",
                    5 => "Good Till Crossing (GTX)",
                    6 => "Good Till Date",
                    7 => "At the Close",
                    _ => string.Empty
                } : string.Empty,
                Time = message.TransactTime.getValue(),
                AvgPx = message.IsSetField(6) ? message.GetDecimal(6) : 0,
                Price = message.IsSetField(44) ? message.GetDecimal(44) : 0,
                TradeSide = message.GetInt(54) == 1 ? "Buy" : "Sell",
                ExpireTime = message.IsSetField(126) ? message.ExpireTime.getValue() : null,
                StopPrice = message.IsSetField(99) ? message.GetDecimal(99) : 0,
                StopLossInPips = message.IsSetField(1003) ? message.GetDecimal(1003) : 0,
                TakeProfitInPips = message.IsSetField(1001) ? message.GetDecimal(1001) : 0,
                TrailingStopLoss = message.IsSetField(1004) ? message.GetBoolean(1004) : null,
                GuaranteedStopLoss = message.IsSetField(1006) ? message.GetBoolean(1006) : null,
                StopLossTriggerMethod = message.IsSetField(1005) ? message.GetInt(1005) switch
                {
                    1 => "Trade Side",
                    2 => "Opposite Side",
                    3 => "Double Trade Side",
                    4 => "Double Opposite Side",
                    _ => string.Empty
                } : string.Empty,
                Text = message.IsSetField(58) ? message.GetString(58) : string.Empty,
                Designation = message.IsSetField(494) ? message.GetString(494) : string.Empty,
                OrderRejReason = message.IsSetField(103) ? message.GetInt(103) switch
                {
                    0 => "Broker / Exchange option",
                    1 => "Unknown symbol",
                    2 => "Exchange closed",
                    3 => "Order exceeds limit",
                    4 => "Too late to enter",
                    5 => "Unknown Order",
                    6 => "Duplicate Order",
                    7 => "Duplicate of a verbally communicated order",
                    8 => "Stale Order",
                    9 => "Trade Along required",
                    10 => "Invalid Investor ID",
                    11 => "Unsupported order characteristic",
                    12 => "Surveillence Option",
                    13 => "Incorrect quantity",
                    14 => "Incorrect allocated quantity",
                    15 => "Unknown account(s)",
                    99 => "Other",
                    _ => string.Empty,
                } : string.Empty,
            };
        }
    }

    public record Symbol(int Id, string Name, int Digits);

    public record SymbolQuote(int SymbolId, string SymbolName, decimal Bid, decimal Ask, int Digits);

    public record PositionUpdate
    {
        public int Index { get; init; }
        public int TotalElements { get; init; }

        public Position Position { get; init; }
        
    }
    public record Position
    {
        public int Index { get; set; }
        public long Id { get; init; }

        public int SymbolId { get; init; }

        public string SymbolName { get; set; }

        public decimal EntryPrice { get; init; }

        public decimal Volume { get; init; }

        public string TradeSide { get; init; }

        public decimal StopLoss { get; init; }

        public decimal TakeProfit { get; init; }

        public bool? TrailingStopLoss { get; init; }

        public string StopLossTriggerMethod { get; init; }

        public bool? GuaranteedStopLoss { get; init; }
    }

    public record Order
    {
        public long Id { get; init; }
        public string ClOrderId { get; init; }
        public string PositionId { get; init; }
        public string OrderStatus { get; set; }
        public string Type { get; init; }

        public int SymbolId { get; init; }

        public string SymbolName { get; set; }

        public decimal TargetPrice { get; init; }
        public decimal AvgPx { get; init; }
        public decimal Price { get; init; }

        public decimal Volume { get; init; }
        public decimal LeavesQty { get; init; }
        public decimal CumQty { get; init; }
        public string TradeSide { get; init; }

        public string TimeInForce { get; init; }
        public DateTime Time { get; init; }

        public DateTime? ExpireTime { get; init; }

        public decimal StopPrice { get; init; }
        public decimal StopLossInPips { get; init; }

        public decimal TakeProfitInPips { get; init; }

        public bool? TrailingStopLoss { get; init; }

        public string StopLossTriggerMethod { get; init; }

        public bool? GuaranteedStopLoss { get; init; }
        public string Text { get;init; }
        public string Designation { get;init; }
        public string OrderRejReason { get;init; }
    }
}