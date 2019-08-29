using System;
using System.Collections.Generic;
using System.Xml;

namespace yae.ECS.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var myEntity = new PlayerEntity();

            //api2
            using (myEntity.Insert(out HealthComponent cmp2))
            {
                cmp2.Life = 148;
                Console.WriteLine(cmp2.Life);
            }

            using (myEntity.Update(out HealthComponent cmp2))
            {
                Console.WriteLine(cmp2.Life);
                cmp2.Life = 150;
                Console.WriteLine(cmp2.Life);
            }
            myEntity.Remove<HealthComponent>();
            Console.ReadLine();
        }
    }

    class HealthComponent
    {
        public int Life { get; set; }
        public int Shield { get; set; }
    }

    class PlayerEntity : Entity<PlayerEntity>
    {
        
    }

    public class MapComponent
    {
        public int MapId { get; set; }
    }

    public class TradeComponent
    {
        public int Money { get; set; }
    }

    public class InventoryComponent
    {
        public int Money { get; set; }
    }

    public class TradeSystem : ISystem<
        (TradeComponent trade, InventoryComponent inv), 
        (TradeComponent trade, InventoryComponent inv )>
    {
        public IEnumerable<Operation> Execute((TradeComponent trade, InventoryComponent inv) source,
            (TradeComponent trade, InventoryComponent inv) destination)
        {
            var (trade, inv) = destination;
            if (source.trade.Money > source.inv.Money ||
                trade.Money > inv.Money)
                yield return Operation.Remove<TradeComponent>(Destination.Both);
        }
    }
}
