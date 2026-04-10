// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace FastWinFormsCharts3D.Demo;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

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

        SuspendLayout();

        // _chartControl
        _chartControl.Dock = DockStyle.Fill;
        _chartControl.BackColor = Color.FromArgb(20, 20, 30);

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        Controls.Add(_chartControl);
        Text = "FastWinFormsCharts3D — Demo";
        StartPosition = FormStartPosition.CenterScreen;

        ResumeLayout(false);
    }
}
