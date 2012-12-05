ncopy
=======

Like copy and xcopy, ncopy is yet another Windows command line copy utility that can copy files and directory trees.

Even better, ncopy can concatenate multiple files into a single encoded file without including the extra byte order marks (BOM) that can invalid your concatenated file.

Examples
--------

To copy an entire directory tree to a new directory:

	ncopy "..\blah" "blech"

To copy a single file to an existing directory:

	ncopy "..\eek.txt" "ack"

To concatenate multiple files into a single ASCII-encoded file:

	ncopy "..\foo.txt"+"..\bar.txt" "foobar.txt" /encoding:ascii

Enjoy!