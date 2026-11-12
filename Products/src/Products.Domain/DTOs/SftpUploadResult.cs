using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.DTOs;

public sealed record SftpUploadResult(
      string Path,
    long Size);
