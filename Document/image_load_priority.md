# Image reading priority order

You can insert an image to markdown.

```md
![attrnm](path/to/image.png)
```

MdXaml attempt to load image from resource or filesystem.

The below example is an image loading rule of MdXaml

1. If you set absolute path, MdXaml read as it is.
2. If you set relative path, MdXaml read from resource.
3. If '2' is failed, MdXaml read from filesystem (In default, current directory).
3. If '3' is failed, MdXaml give up.