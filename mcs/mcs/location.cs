//
// location.cs: Keeps track of the location of source code entity
//
// Author:
//   Miguel de Icaza
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright 2001 Ximian, Inc.
// Copyright 2005 Novell, Inc.
//

using System;
using System.IO;
using System.Collections;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {
	/// <summary>
	///   This is one single source file.
	/// </summary>
	/// <remarks>
	///   This is intentionally a class and not a struct since we need
	///   to pass this by reference.
	/// </remarks>
	public class SourceFile : ISourceFile {
		public readonly string Name;
		public readonly string Path;
		public readonly int Index;
		public bool AutoGenerated;
		public bool IsIncludeFile;

		SourceFileEntry file;
		byte[] guid, checksum;

		public SourceFile (string name, string path, int index, bool is_include)
		{
			this.Index = index;
			this.Name = name;
			this.Path = path;
			this.IsIncludeFile = is_include;
		}

		public SourceFileEntry SourceFileEntry {
			get { return file; }
		}

		SourceFileEntry ISourceFile.Entry {
			get { return file; }
		}

		public void SetChecksum (byte[] guid, byte[] checksum)
		{
			this.guid = guid;
			this.checksum = checksum;
		}

		public virtual void DefineSymbolInfo (MonoSymbolWriter symwriter)
		{
			if (guid != null)
				file = symwriter.DefineDocument (Path, guid, checksum);
			else {
				file = symwriter.DefineDocument (Path);
				if (AutoGenerated)
					file.SetAutoGenerated ();
			}
		}

		public override string ToString ()
		{
			return String.Format ("SourceFile ({0}:{1}:{2}:{3})",
					      Name, Path, Index, SourceFileEntry);
		}
	}

	public class CompilationUnit : SourceFile, ICompileUnit
	{
		CompileUnitEntry comp_unit;
		Hashtable include_files;
		Hashtable conditionals;

		public CompilationUnit (string name, string path, int index)
			: base (name, path, index, false)
		{ }

		public void AddFile (SourceFile file)
		{
			if (file == this)
				return;
			
			if (include_files == null)
				include_files = new Hashtable ();

			if (!include_files.Contains (file.Path))
				include_files.Add (file.Path, file);
		}

		public void AddDefine (string value)
		{
			if (conditionals == null)
				conditionals = new Hashtable (2);

			conditionals [value] = true;
		}

		public void AddUndefine (string value)
		{
			if (conditionals == null)
				conditionals = new Hashtable (2);

			conditionals [value] = null;
		}

		CompileUnitEntry ICompileUnit.Entry {
			get { return comp_unit; }
		}

		public CompileUnitEntry CompileUnitEntry {
			get { return comp_unit; }
		}

		public override void DefineSymbolInfo (MonoSymbolWriter symwriter)
		{
			base.DefineSymbolInfo (symwriter);

			comp_unit = symwriter.DefineCompilationUnit (SourceFileEntry);

			if (include_files != null) {
				foreach (SourceFile include in include_files.Values) {
					include.DefineSymbolInfo (symwriter);
					comp_unit.AddFile (include.SourceFileEntry);
				}
			}
		}

		public bool IsConditionalDefined (string value)
		{
			if (conditionals != null) {
				object res = conditionals [value];
				if (res != null)
					return (bool)res;
				
				// When conditional was undefined
				if (conditionals.Contains (value))
					return false;					
			}

			return RootContext.IsConditionalDefined (value);
		}
	}

	/// <summary>
	///   Keeps track of the location in the program
	/// </summary>
	///
	/// <remarks>
	///   This uses a compact representation and a couple of auxiliary
	///   structures to keep track of tokens to (file,line and column) 
	///   mappings. The usage of the bits is:
	///   
	///     - 16 bits for "checkpoint" which is a mixed concept of
	///       file and "line segment"
	///     - 8 bits for line delta (offset) from the line segment
	///     - 8 bits for column number.
	///
	///   http://lists.ximian.com/pipermail/mono-devel-list/2004-December/009508.html
	/// </remarks>
	public struct Location {
		int token; 

		struct Checkpoint {
			public readonly int LineOffset;
			public readonly int CompilationUnit;
			public readonly int File;

			public Checkpoint (int compile_unit, int file, int line)
			{
				File = file;
				CompilationUnit = compile_unit;
				LineOffset = line - (int) (line % (1 << line_delta_bits));
			}
		}

		static ArrayList source_list;
		static ArrayList compile_units;
		static Hashtable source_files;
		static int checkpoint_bits;
		static int source_count;
		static int current_source;
		static int current_compile_unit;
		static int line_delta_bits;
		static int line_delta_mask;
		static int column_bits;
		static int column_mask;
		static Checkpoint [] checkpoints;
		static int checkpoint_index;
		
		public readonly static Location Null = new Location (-1);
		public static bool InEmacs;
		
		static Location ()
		{
			source_files = new Hashtable ();
			source_list = new ArrayList ();
			compile_units = new ArrayList ();
			current_source = 0;
			current_compile_unit = 0;
			checkpoints = new Checkpoint [10];
		}

		public static void Reset ()
		{
			source_files = new Hashtable ();
			source_list = new ArrayList ();
			compile_units = new ArrayList ();
			current_source = 0;
			current_compile_unit = 0;
			source_count = 0;
		}

		// <summary>
		//   This must be called before parsing/tokenizing any files.
		// </summary>
		static public void AddFile (Report r, string name)
		{
			string path = Path.GetFullPath (name);

			if (source_files.Contains (path)){
				int id = (int) source_files [path];
				string other_name = ((SourceFile) source_list [id - 1]).Name;
				if (name.Equals (other_name))
					r.Warning (2002, 1, "Source file `{0}' specified multiple times", other_name);
				else
					r.Warning (2002, 1, "Source filenames `{0}' and `{1}' both refer to the same file: {2}", name, other_name, path);
				return;
			}

			source_files.Add (path, ++source_count);
			CompilationUnit unit = new CompilationUnit (name, path, source_count);
			source_list.Add (unit);
			compile_units.Add (unit);
		}

		// IList<CompilationUnit>
		static public ArrayList SourceFiles {
			get {
				return compile_units;
			}
		}

		// <summary>
		//   After adding all source files we want to compile with AddFile(), this method
		//   must be called to `reserve' an appropriate number of bits in the token for the
		//   source file.  We reserve some extra space for files we encounter via #line
		//   directives while parsing.
		// </summary>
		static public void Initialize ()
		{
			checkpoints = new Checkpoint [source_list.Count * 2];
			if (checkpoints.Length > 0)
				checkpoints [0] = new Checkpoint (0, 0, 0);

			column_bits = 8;
			column_mask = 0xFF;
			line_delta_bits = 8;
			line_delta_mask = 0xFF00;
			checkpoint_index = 0;
			checkpoint_bits = 16;
		}

		// <remarks>
		//   This is used when we encounter a #line preprocessing directive.
		// </remarks>
		static public SourceFile LookupFile (CompilationUnit comp_unit, string name)
		{
			string path;
			if (!Path.IsPathRooted (name)) {
				string root = Path.GetDirectoryName (comp_unit.Path);
				path = Path.Combine (root, name);
			} else
				path = name;

			if (!source_files.Contains (path)) {
				if (source_count >= (1 << checkpoint_bits))
					return new SourceFile (name, path, 0, true);

				source_files.Add (path, ++source_count);
				SourceFile retval = new SourceFile (name, path, source_count, true);
				source_list.Add (retval);
				return retval;
			}

			int index = (int) source_files [path];
			return (SourceFile) source_list [index - 1];
		}

		static public void Push (CompilationUnit compile_unit, SourceFile file)
		{
			current_source = file != null ? file.Index : -1;
			current_compile_unit = compile_unit != null ? compile_unit.Index : -1;
			// File is always pushed before being changed.
		}

		// <remarks>
		//   If we're compiling with debugging support, this is called between parsing
		//   and code generation to register all the source files with the
		//   symbol writer.
		// </remarks>
		static public void DefineSymbolDocuments (MonoSymbolWriter symwriter)
		{
			foreach (CompilationUnit unit in compile_units)
				unit.DefineSymbolInfo (symwriter);
		}
		
		public Location (int row)
			: this (row, 0)
		{
		}

		public Location (int row, int column)
		{
			if (row <= 0)
				token = 0;
			else {
				if (column > 254)
					column = 254;
				if (column < 0)
					column = 255;
				int target = -1;
				int delta = 0;
				int max = checkpoint_index < 10 ?
					checkpoint_index : 10;
				for (int i = 0; i < max; i++) {
					int offset = checkpoints [checkpoint_index - i].LineOffset;
					delta = row - offset;
					if (delta >= 0 &&
						delta < (1 << line_delta_bits) &&
						checkpoints [checkpoint_index - i].File == current_source) {
						target = checkpoint_index - i;
						break;
					}
				}
				if (target == -1) {
					AddCheckpoint (current_compile_unit, current_source, row);
					target = checkpoint_index;
					delta = row % (1 << line_delta_bits);
				}
				long l = column +
					(long) (delta << column_bits) +
					(long) (target << (line_delta_bits + column_bits));
				token = l > 0xFFFFFFFF ? 0 : (int) l;
			}
		}

		static void AddCheckpoint (int compile_unit, int file, int row)
		{
			if (checkpoints.Length == ++checkpoint_index) {
				Checkpoint [] tmp = new Checkpoint [checkpoint_index * 2];
				Array.Copy (checkpoints, tmp, checkpoints.Length);
				checkpoints = tmp;
			}
			checkpoints [checkpoint_index] = new Checkpoint (compile_unit, file, row);
		}
		
		public override string ToString ()
		{
			if (column_bits == 0 || InEmacs)
				return Name + "(" + Row.ToString () + "):";
			else
				return Name + "(" + Row.ToString () + "," + Column.ToString () +
					(Column == column_mask ? "+):" : "):");
		}
		
		/// <summary>
		///   Whether the Location is Null
		/// </summary>
		public bool IsNull {
			get { return token == 0; }
		}

		public string Name {
			get {
				int index = File;
				if (token == 0 || index == 0)
					return "Internal";

				SourceFile file = (SourceFile) source_list [index - 1];
				return file.Name;
			}
		}

		int CheckpointIndex {
			get { return (int) ((token & 0xFFFF0000) >> (line_delta_bits + column_bits)); }
		}

		public int Row {
			get {
				if (token == 0)
					return 1;
				return checkpoints [CheckpointIndex].LineOffset + ((token & line_delta_mask) >> column_bits);
			}
		}

		public int Column {
			get {
				if (token == 0)
					return 1;
				int col = (int) (token & column_mask);
				return col == 255 ? 1 : col;
			}
		}

		public bool Hidden {
			get {
				return (int) (token & column_mask) == 255;
			}
		}

		public int CompilationUnitIndex {
			get {
				if (token == 0)
					return 0;
if (checkpoints.Length <= CheckpointIndex) throw new Exception (String.Format ("Should not happen. Token is {0:X04}, checkpoints are {1}, index is {2}", token, checkpoints.Length, CheckpointIndex));
				return checkpoints [CheckpointIndex].CompilationUnit;
			}
		}

		public int File {
			get {
				if (token == 0)
					return 0;
if (checkpoints.Length <= CheckpointIndex) throw new Exception (String.Format ("Should not happen. Token is {0:X04}, checkpoints are {1}, index is {2}", token, checkpoints.Length, CheckpointIndex));
				return checkpoints [CheckpointIndex].File;
			}
		}

		// The ISymbolDocumentWriter interface is used by the symbol writer to
		// describe a single source file - for each source file there's exactly
		// one corresponding ISymbolDocumentWriter instance.
		//
		// This class has an internal hash table mapping source document names
		// to such ISymbolDocumentWriter instances - so there's exactly one
		// instance per document.
		//
		// This property returns the ISymbolDocumentWriter instance which belongs
		// to the location's source file.
		//
		// If we don't have a symbol writer, this property is always null.
		public SourceFile SourceFile {
			get {
				int index = File;
				if (index == 0)
					return null;
				return (SourceFile) source_list [index - 1];
			}
		}

		public CompilationUnit CompilationUnit {
			get {
				int index = CompilationUnitIndex;
				if (index == 0)
					return null;
				return (CompilationUnit) source_list [index - 1];
			}
		}
	}

	public class LocatedToken
	{
		public readonly Location Location;
		public readonly string Value;

		public LocatedToken (Location loc, string value)
		{
			Location = loc;
			Value = value;
		}

		public override string ToString ()
		{
			return Location.ToString () + Value;
		}
	}
}
