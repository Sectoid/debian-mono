#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at xvision.com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace System.Web.Configuration.nBrowser
{
	using System;
	using System.Runtime.Serialization;
	[
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"),
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")
	]
	internal class Exception : System.Exception
	{
		public Exception()
			: base()
		{
		}
		public Exception(string errorMessage)
			: base(errorMessage)
		{
		}
		public Exception(string message, Exception innerException)
			: base(message, innerException)
		{
			// Add any type-specific logic for inner exceptions.
		}
		protected Exception(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			// Implement type-specific serialization constructor logic.
		}
	}
}
/*
Turns off the following Warning in FxCop
--------------------------------------------------------------------------------------------------------------------
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
The System.Runtime.Serialization.ISerializable interface allows the type to customize its serialization, while 
the Serializable attribute enables the common language runtime to recognize the type as being serializable."
--------------------------------------------------------------------------------------------------------------------
System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")
Multiple constructors are required to correctly implement a custom exception. Missing constructors can make your 
exception unusable in certain scenarios. For example, the serialization constructor is required for handling 
exceptions in XML Web services.
--------------------------------------------------------------------------------------------------------------------
*/
#endif
