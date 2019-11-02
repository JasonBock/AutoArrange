using System;

namespace AutoArrange.Tests
{
	public class SortedClass
	{
		public void Two() { }
		/// <summary>
		/// What happens to comments?
		/// </summary>
		public void One() { }
	}

	public struct SortedStruct
	{
		public void YourMethod() { }
		public Guid AField;
	}

	public sealed class SortedClassWithNestedClass
	{
		public string AProperty { get; set; }

		private int someProperty;
		internal string yetAnotherField;

		public SortedClassWithNestedClass(Guid value) { }
		public void Method() { }
		public Guid MyProperty { get; private set; }

		public int SomeProperty
		{
			get;
			internal set;
		}

		internal event EventHandler LoudEvent;

		public SortedClassWithNestedClass() { }

		public class InternalSortedClass
		{
			public void B() { }
			public void X() { }
		}

		public enum DataValues { A, B, C }
	}
}
