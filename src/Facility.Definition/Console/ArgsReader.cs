﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Console
{
	/// <summary>
	/// Helps process command-line arguments.
	/// </summary>
	public sealed class ArgsReader
	{
		/// <summary>
		/// Creates a reader for the specified command-line arguments.
		/// </summary>
		public ArgsReader(IReadOnlyList<string> args)
		{
			m_args = args.ToList();
		}

		/// <summary>
		/// Reads the specified flag, returning true if it is found.
		/// </summary>
		public bool ReadFlag(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Flag name must not be empty.", nameof(name));

			int index = m_args.IndexOf(RenderOption(name));
			if (index == -1)
				return false;

			m_args.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Reads the specified option, returning null if it is missing.
		/// </summary>
		public string ReadOption(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Option name must not be empty.", nameof(name));

			int index = m_args.IndexOf(RenderOption(name));
			if (index == -1)
				return null;

			if (index + 1 >= m_args.Count)
				throw new ArgsReaderException($"Missing value after '{RenderOption(name)}'.");

			string value = m_args[index + 1];
			if (value[0] == '-')
				throw new ArgsReaderException($"Missing value after '{RenderOption(name)}'.");

			m_args.RemoveAt(index);
			m_args.RemoveAt(index);
			return value;
		}

		/// <summary>
		/// Reads the next non-option argument, or null if none remain.
		/// </summary>
		public string ReadArgument()
		{
			if (m_args.Count == 0)
				return null;

			string value = m_args[0];
			if (value[0] == '-')
				throw new ArgsReaderException($"Unexpected option '{value}'.");

			m_args.RemoveAt(0);
			return value;
		}

		/// <summary>
		/// Confirms that all arguments were processed.
		/// </summary>
		public void VerifyComplete()
		{
			if (m_args.Count != 0)
				throw new ArgsReaderException($"Unexpected {(m_args[0][0] == '-' ? "option" : "argument")} '{m_args[0]}'.");
		}

		private static string RenderOption(string name)
		{
			return name.Length == 1 ? $"-{name}" : $"--{name}";
		}

		readonly List<string> m_args;
	}
}
