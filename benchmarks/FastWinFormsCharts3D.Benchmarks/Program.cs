// Copyright (c) 2026 MabinogiCode. All rights reserved.

using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
