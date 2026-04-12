// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace FastWinFormsCharts3D.Demo;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Timer _animTimer = null!;
    private TabControl _tabControl = null!;
    private TabPage _tabScatter = null!;
    private TabPage _tabSurface = null!;
    private TabPage _tabBar = null!;
    private Chart3DControl _surfaceControl = null!;
    private Chart3DControl _barControl = null!;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _chartControl = new Chart3DControl();
        _surfaceControl = new Chart3DControl();
        _barControl = new Chart3DControl();
        _animTimer = new System.Windows.Forms.Timer(components);
        _tabControl = new TabControl();
        _tabScatter = new TabPage();
        _tabSurface = new TabPage();
        _tabBar = new TabPage();

        SuspendLayout();

        // _animTimer — ~60 fps, tick handler in MainForm.cs
        _animTimer.Interval = 16;
        _animTimer.Tick += OnAnimTimerTick;

        // _chartControl (scatter)
        _chartControl.Dock = DockStyle.Fill;
        _chartControl.BackColor = Color.FromArgb(20, 20, 30);

        // _surfaceControl
        _surfaceControl.Dock = DockStyle.Fill;
        _surfaceControl.BackColor = Color.FromArgb(20, 20, 30);

        // _barControl
        _barControl.Dock = DockStyle.Fill;
        _barControl.BackColor = Color.FromArgb(20, 20, 30);

        // _tabScatter
        _tabScatter.Text = "Scatter 3D";
        _tabScatter.BackColor = Color.FromArgb(20, 20, 30);
        _tabScatter.Controls.Add(_chartControl);

        // _tabSurface
        _tabSurface.Text = "Surface 3D";
        _tabSurface.BackColor = Color.FromArgb(20, 20, 30);
        _tabSurface.Controls.Add(_surfaceControl);

        // _tabBar
        _tabBar.Text = "Bar 3D";
        _tabBar.BackColor = Color.FromArgb(20, 20, 30);
        _tabBar.Controls.Add(_barControl);

        // _tabControl
        _tabControl.Dock = DockStyle.Fill;
        _tabControl.TabPages.Add(_tabScatter);
        _tabControl.TabPages.Add(_tabSurface);
        _tabControl.TabPages.Add(_tabBar);

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 820);
        Controls.Add(_tabControl);
        Text = "FastWinFormsCharts3D — Demo";
        StartPosition = FormStartPosition.CenterScreen;

        ResumeLayout(false);
    }
}
