using System;

public class MyClass
{
	public event EventHandler MyEvent;
	public enum MyEnum { A };
	public MyClass() { }
	public string MyProperty { get; set; }
	public void MyMethod() { }
	public class MyNestedClass { }
	public string MyField;
}