// CS0178: Invalid rank specifier: expected `,' or `]'
// Line: 8

public class Blah {
	
	public static void Main ()
	{
		int [,] i = new int [4,2] { 0, 1, 2, 3, 4, 5 };
	}
}
