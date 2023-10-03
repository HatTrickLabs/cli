using System.Collections;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class SetOfTests
    {
        #region constructor tests
        [Fact]
        public void Constructor_Parameterless_CapacityShouldBe0()
        {
            //allowed capacities: 0,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
            //................... 16384,32768,65536,131072,262144,524288,1048576

            var set = new SetOf<string>();

            Assert.Equal(0, set.Capacity);
        }

        [Fact]
        public void Constructor_Parameterless_LengthShouldBe0()
        {
            var set = new SetOf<string>();

            Assert.Equal(0, set.Length);
        }

        [Fact]
        public void Constructor_Parameterless_ItemsShouldEqualEmptyArray()
        {
            var set = new SetOf<string>();

            Assert.Equal(Array.Empty<string>(), set.ToArray());
        }

        [Fact]
        public void Constructor_MinimumCapacity_ShouldBe_GreaterThanOrEqual_ProvidedCapacity()
        {
            //allowed capacities: 0,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
            //................... 16384,32768,65536,131072,262144,524288,1048576
            int capacity = 5;
            var set = new SetOf<string>(capacity);

            Assert.True(set.Capacity >= capacity);
        }

        [Fact]
        public void Constructor_MaximumCapacityPlusOne_ShouldThrow_RangeOverflowException()
        {
            //allowed capacities: 0,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
            //................... 16384,32768,65536,131072,262144,524288,1048576
            int capacity = SetOf<string>.MaxCapacity + 1;

            SetOf<string> set; 

            Assert.Throws<RangeOverflowException>(() => set = new SetOf<string>(capacity));
        }

        [Fact]
        public void Constructor_MinimumCapacity_LessThan0_ShouldThrow_ArgumentOutOfRangeException()
        {
            //allowed capacities: 0,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
            //................... 16384,32768,65536,131072,262144,524288,1048576
            int capacity = -1;

            SetOf<string> set;

            Assert.Throws<ArgumentOutOfRangeException>(() => set = new SetOf<string>(capacity));
        }

        [Fact]
        public void Constructor_MinimumCapacity_RollsToPowerOf2()
        {
            int min = 10; //should roll actual capcity to 16
            int actual = 16;
            var set = new SetOf<string>(min);

            Assert.Equal(actual, set.Capacity);
        }

        [Fact]
        public void Constructor_Items_StoredInCorrectOrder()
        {
            var items = new string[] { "uno", "dos", "tres" };

            var set = new SetOf<string>(items);

            Assert.True(set[0] == "uno" && set[1] == "dos" && set[2] == "tres");
        }

        [Fact]
        public void Constructor_Items_MinCapacityShouldBePowerOf2()
        {
            var items = new string[] { "uno", "dos", "tres" };

            var set = new SetOf<string>(items);

            Assert.Equal(4, set.Capacity);
        }

        [Fact]
        public void Constructor_Items_LengthShouldEqualItemsLength()
        {
            var items = new string[] { "uno", "dos", "tres" };

            var set = new SetOf<string>(items);

            Assert.Equal(3, set.Length);
        }

        [Fact]
        public void Add_WhenCapacityIs0_CapacityShouldIncrementTo4()
        {
            //allowed capacities: 0,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
            //................... 16384,32768,65536,131072,262144,524288,1048576

            var set = new SetOf<string>();

            set.Add("x");

            Assert.Equal(4, set.Capacity);
        }

        [Fact]
        public void Add_WhenCapacityIs0_LengthShouldIncrementTo1()
        {
            var set = new SetOf<string>();

            set.Add("x");

            Assert.Equal(1, set.Length);
        }

        [Fact]
        public void Add_WhenItemsExist_AndLengthEqualsCapacity_CapacityShouldDouble()
        {
            int initialCapacity = 16;
            var set = new SetOf<string>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                set.Add("x");
            }

            Assert.Equal(initialCapacity, set.Capacity);
            set.Add("x");
            Assert.Equal(initialCapacity * 2, set.Capacity);
        }

        [Fact]
        public void Add_WhenItemsExist_AndLengthEqualsCapacity_LengthShouldIncrementBy1()
        {
            int initialCapacity = 4;
            var set = new SetOf<string>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                set.Add("x");
            }

            Assert.Equal(initialCapacity, set.Capacity);
            set.Add("x");
            Assert.Equal(initialCapacity + 1, set.Length);
        }

        [Fact]
        public void Add_WhenItemsExist_AndLengthEqualsCapacity_ItemsShouldBeCopiedToNewBuffer_OrderMaintained()
        {
            int initialCapacity = 4;
            var set = new SetOf<string>(initialCapacity);

            
            set.Add("0");
            set.Add("1");
            set.Add("2");
            set.Add("3");


            Assert.Equal(initialCapacity, set.Length);
            set.Add("x");
            Assert.Equal(initialCapacity + 1, set.Length);

            Assert.Equal("0", set[0]);
            Assert.Equal("1", set[1]);
            Assert.Equal("2", set[2]);
            Assert.Equal("3", set[3]);
            Assert.Equal("x", set[4]);
        }

        [Fact]
        public void Add_WhenLengthIsMaxCapacity_ShouldThrow_RangeOverflowException()
        {
            var set = new SetOf<int>(SetOf<int>.MaxCapacity);
            for (int i = 0; i < set.Capacity; i++)
            {
                set.Add(0);
            }

            Assert.Throws<RangeOverflowException>(() => set.Add(0));
        }
        #endregion

        #region exists tests
        [Fact]
        public void Exists_WhenIsEmpty_ShouldReturn_False()
        {
            var set = new SetOf<string>();

            Assert.False(set.Exists((x) => true));
        }

        [Fact]
        public void Exists_WhenPopulated_IfNoPredicateMatch_ShouldReturn_False()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.False(set.Exists((x) => x == "abc"));
        }

        [Fact]
        public void Exists_WhenPopulated_IfPredicateMatch_ShouldReturn_True()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.True(set.Exists((x) => x == "dos"));
        }

        [Fact]
        public void Exists_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.Exists(null));
        }
        #endregion

        #region find index
        [Fact]
        public void FindIndex_WhenIsEmpty_ShouldReturn_Negative1()
        {
            var set = new SetOf<string>();

            Assert.Equal(-1, set.FindIndex((x) => true));
        }

        [Fact]
        public void FindIndex_WhenIsPopulated_IfNoPredicateMatch_ShouldReturn_Negative1()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Equal(-1, set.FindIndex((x) => x == "abc"));
        }

        [Fact]
        public void FindIndex_WhenIsPopulated_IfPredicateMatch_ShouldReturn_IndexOfMatch()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Equal(2, set.FindIndex((x) => x == "tres"));
        }
        
        [Fact]
        public void FindIndex_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.FindIndex(null));
        }
        #endregion

        #region find
        [Fact]
        public void Find_WhenIsEmpty_ShouldReturn_DefaultOfT()
        {
            var set = new SetOf<int>();

            Assert.Equal(default(int), set.Find((x) => true));
        }

        [Fact]
        public void Find_WhenIsPopulated_IfNoPredicateMatch_ShouldReturn_DefaultOfT()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Equal(default(string), set.Find((x) => x == "abc"));
        }

        [Fact]
        public void Find_WhenIsPopulated_IfPredicateMatch_ShouldReturn_MatchedItem()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Equal("dos", set.Find((x) => x == "dos"));
        }

        [Fact]
        public void Find_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.Find(null));
        }
        #endregion

        #region find all
        [Fact]
        public void FindAll_WhenIsEmpty_ShouldReturn_EmptyArrayOfT()
        {
            var set = new SetOf<int>();

            Assert.Equal(Array.Empty<int>(), set.FindAll((x) => true));
        }

        [Fact]
        public void FindAll_WhenIsPopulated_IfNoPredicateMatch_ShouldReturn_EmptyArrayOfT()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Equal(Array.Empty<string>(), set.FindAll((x) => x == "abc"));
        }

        [Fact]
        public void FindAll_WhenIsPopulated_IfPredicateMatch_ShouldReturn_ArrayOfMatchedItems_InMatchedItemOrder()
        {
            var set = new SetOf<int>(new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 });

            var result = set.FindAll((x) => x >= 10 && x != 12);
            Assert.Collection<int>(result,
                x => Assert.Equal(10, x),
                x => Assert.Equal(11, x),
                x => Assert.Equal(13, x)
            );
        }

        [Fact]
        public void FindAll_WhenIsPopulatedWithExactly1_IfPredicateMatch_ShouldReturn_ArrayOfSingleMatchedItem()
        {
            var set = new SetOf<int>(new int[] { 8 });

            var result = set.FindAll((x) => x == 8);
            Assert.Collection<int>(result, x => Assert.Equal(8, x));
        }

        [Fact]
        public void FindAll_WhenIsPopulatedMoreThanStackAllocOf1024_IfPredicateMatch_ShouldReturn_ArrayOfMatchedItems_InMatchedItemOrder()
        {
            int capacity = 1500;
            var set = new SetOf<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                set.Add(i);
            }

            var result = set.FindAll((x) => x >= 1495);
            Assert.Collection<int>(result,
                x => Assert.Equal(1495, x),
                x => Assert.Equal(1496, x),
                x => Assert.Equal(1497, x),
                x => Assert.Equal(1498, x),
                x => Assert.Equal(1499, x)
            );
        }

        [Fact]
        public void FindAll_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.FindAll(null));
        }
        #endregion

        #region max
        [Fact]
        public void Max_WhenIsEmpty_ShouldReturn_DefaultOfT()
        {
            var set = new SetOf<int>();

            Assert.Equal(default(int), set.Max((x) => x));
        }

        [Fact]
        public void Max_WhenContainsExactly1Item_ShouldReturn_TheOneItem()
        {
            var set = new SetOf<int>();
            set.Add(100);

            Assert.Equal(100, set.Max((x) => x));
        }

        [Fact]
        public void Max_WhenContainsManyItems_ShouldReturn_TheMaximumValueAssumingDefaultComparer()
        {
            int capacity = 32;
            var set = new SetOf<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                set.Add(i * 2);
            }

            Assert.Equal(62, set.Max((x) => x));
        }

        [Fact]
        public void Max_WhenContainsManyProjectedItems_ShouldReturn_TheMaximumProjectedValueAssumingDefaultComparer()
        {
            int capacity = 32;
            var set = new SetOf<string>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                set.Add(new string('x', i + 1));
            }
            //get max string len
            Assert.Equal(32, set.Max((x) => x.Length));
        }

        [Fact]
        public void Max_WhenContainsManyStringItems_ShouldReturn_TheMaximumLengthStringItem()
        {
            int capacity = 32;
            var set = new SetOf<string>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                set.Add(new string('x', i + 1));
            }
            //get max string len
            Assert.Equal(new string('x', 32), set.Max((x) => x));
        }

        [Fact]
        public void Max_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.Max(null as Func<string, int>));
        }
        #endregion

        #region min
        [Fact]
        public void Min_WhenIsEmpty_ShouldReturn_DefaultOfT()
        {
            var set = new SetOf<int>();

            Assert.Equal(default(int), set.Min((x) => x));
        }

        [Fact]
        public void Min_WhenContainsExactly1Item_ShouldReturn_TheOneItem()
        {
            var set = new SetOf<int>();
            set.Add(100);

            Assert.Equal(100, set.Min((x) => x));
        }

        [Fact]
        public void Min_WhenContainsManyItems_ShouldReturn_TheMinimumValueAssumingDefaultComparer()
        {
            int capacity = 32;
            var set = new SetOf<int>(capacity);
            for (int i = capacity - 1; i >= 0; i--)
            {
                set.Add(i * 2);
            }

            Assert.Equal(0, set.Min((x) => x));
        }

        [Fact]
        public void Min_WhenContainsManyProjectedItems_ShouldReturn_TheMinimumProjectedValueAssumingDefaultComparer()
        {
            int capacity = 32;
            var set = new SetOf<string>(capacity);
            for (int i = capacity - 1; i >= 0; i--)
            {
                set.Add(new string('x', i + 1));
            }
            //get max string len
            Assert.Equal(1, set.Min((x) => x.Length));
        }

        [Fact]
        public void Min_WhenContainsManyStringItems_ShouldReturn_TheMinimumLengthStringItem()
        {
            int capacity = 32;
            var set = new SetOf<string>(capacity);
            for (int i = capacity - 1; i >= 0; i--)
            {
                set.Add(new string('x', i + 1));
            }
            
            Assert.Equal(new string('x', 1), set.Min((x) => x));
        }

        [Fact]
        public void Min_IfPredicateProvided_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOf<string>(new string[] { "uno", "dos", "tres" });

            Assert.Throws<ArgumentNullException>(() => set.Min(null as Func<string, int>));
        }
        #endregion

        #region to array
        [Fact]
        public void ToArray_WhenIsEmpty_ShouldReturn_EmptyArrayOfT()
        {
            var set = new SetOf<string>();

            Assert.Equal(Array.Empty<string>(), set.ToArray());
        }

        [Fact]
        public void ToArray_WhenHasExactlyOneItem_ShouldReturn_ArrayLengthOne_WithTheOneItem()
        {
            var set = new SetOf<string>();
            set.Add("abc");

            string[] array = set.ToArray();
            Assert.Collection<string>(array, (x) => Assert.Equal("abc", x));
        }

        [Fact]
        public void ToArray_WhenHasExactlyTwoItems_ShouldReturn_ArrayLengthTwo_WithTheTwoItems()
        {
            var set = new SetOf<string>();
            set.Add("abc");
            set.Add("def");

            string[] array = set.ToArray();
            Assert.Collection<string>(array, (x) => Assert.Equal("abc", x), (x) => Assert.Equal("def", x));
        }

        [Fact]
        public void ToArray_WhenHasExactlyThreeItems_ShouldReturn_ArrayLengthThree_WithTheThreeItems()
        {
            var set = new SetOf<string>();
            set.Add("abc");
            set.Add("def");
            set.Add("xyz");

            string[] array = set.ToArray();
            Assert.Collection<string>(array, 
                (x) => Assert.Equal("abc", x), 
                (x) => Assert.Equal("def", x), 
                (x) => Assert.Equal("xyz", x)
            );
        }
        #endregion

        #region ienumerator<T>
        [Fact]
        public void IEnumerableOfT_GetEnumerator_WhenIsEmpty_ShouldNotMoveNext()
        {
            var set = new SetOf<string>();

            var enumerator = (set as IEnumerable<string>).GetEnumerator();

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void IEnumerableOfT_GetEnumerator_WhenContainsSingleItem_ShouldMoveNextOnce()
        {
            var set = new SetOf<string>();
            set.Add("one");

            var enumerator = (set as IEnumerable<string>).GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void IEnumerableOfT_WhenIsPopulated_ShouldReturn_AllItemsInProperOrder()
        {
            var set = new SetOf<string>();
            set.Add("one");
            set.Add("two");
            set.Add("three");
            set.Add("four");
            set.Add("five");

            var enumerator = (set as IEnumerable<string>).GetEnumerator();

            enumerator.MoveNext();
            Assert.Equal("one", enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("two", enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("three", enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("four", enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("five", enumerator.Current);
        }

        [Fact]
        public void IEnumerableOfT_WhenIsPopulated_ShouldReturn_AllItemsInProperOrder_ViaForeachIterator()
        {
            var set = new SetOf<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);
            set.Add(5);

            var enumerator = (set as IEnumerable<int>);

            int i = 0;
            foreach (var item in enumerator)
            {
                Assert.Equal(++i, item);
            }
        }
        #endregion

        #region ienumerator
        [Fact]
        public void IEnumerable_GetEnumerator_WhenIsEmpty_ShouldNotMoveNext()
        {
            var set = new SetOf<string>();

            var enumerator = (set as IEnumerable).GetEnumerator();

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void IEnumerable_GetEnumerator_WhenContainsSingleItem_ShouldMoveNextOnce()
        {
            var set = new SetOf<string>();
            set.Add("one");

            var enumerator = (set as IEnumerable).GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void IEnumerable_WhenIsPopulated_ShouldReturn_AllItemsInProperOrder()
        {
            var set = new SetOf<string>();
            set.Add("one");
            set.Add("two");
            set.Add("three");
            set.Add("four");
            set.Add("five");

            var enumerator = (set as IEnumerable).GetEnumerator();

            enumerator.MoveNext();
            Assert.Equal("one", (string)enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("two", (string)enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("three", (string)enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("four", (string)enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal("five", (string)enumerator.Current);
        }

        [Fact]
        public void IEnumerable_WhenIsPopulated_ShouldReturn_AllItemsInProperOrder_ViaForeachIterator()
        {
            var set = new SetOf<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);
            set.Add(5);

            var enumerator = (set as IEnumerable);

            int i = 0;
            foreach (var item in enumerator)
            {
                Assert.Equal(++i, (int)item);
            }
        }
        #endregion
    }
}