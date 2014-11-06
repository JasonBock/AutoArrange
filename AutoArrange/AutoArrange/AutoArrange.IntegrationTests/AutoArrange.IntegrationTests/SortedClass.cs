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
		public enum DataValues { A, B, C }
		private int someProperty;

		public SortedClassWithNestedClass(Guid value) { }
		public Guid MyProperty { get; private set; }
		internal string yetAnotherField;
		public SortedClassWithNestedClass() { }
		public void Method() { }
		public string AProperty { get; set; }

		public class InternalSortedClass
		{
			public void X() { }
			public void B() { }
		}

		internal event EventHandler LoudEvent;

		public int SomeProperty
		{
			get;
			internal set;
		}
	}
}
