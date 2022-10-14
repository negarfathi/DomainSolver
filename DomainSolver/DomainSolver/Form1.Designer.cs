namespace DomainSolver
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelProjectPath = new System.Windows.Forms.Label();
            this.textBoxProjectPath = new System.Windows.Forms.TextBox();
            this.buttonProjectPath = new System.Windows.Forms.Button();
            this.labelClassName = new System.Windows.Forms.Label();
            this.textBoxClassName = new System.Windows.Forms.TextBox();
            this.labelMethodName = new System.Windows.Forms.Label();
            this.textBoxMethodName = new System.Windows.Forms.TextBox();
            this.labelPathConstraint = new System.Windows.Forms.Label();
            this.textBoxPathConstraint = new System.Windows.Forms.TextBox();
            this.buttonPathConstraint = new System.Windows.Forms.Button();
            this.buttonSolve = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelProjectPath
            // 
            this.labelProjectPath.AutoSize = true;
            this.labelProjectPath.Location = new System.Drawing.Point(46, 45);
            this.labelProjectPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelProjectPath.Name = "labelProjectPath";
            this.labelProjectPath.Size = new System.Drawing.Size(87, 19);
            this.labelProjectPath.TabIndex = 0;
            this.labelProjectPath.Text = "Project Path:";
            // 
            // textBoxProjectPath
            // 
            this.textBoxProjectPath.Location = new System.Drawing.Point(157, 42);
            this.textBoxProjectPath.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxProjectPath.Name = "textBoxProjectPath";
            this.textBoxProjectPath.Size = new System.Drawing.Size(499, 26);
            this.textBoxProjectPath.TabIndex = 1;
            // 
            // buttonProjectPath
            // 
            this.buttonProjectPath.Location = new System.Drawing.Point(664, 40);
            this.buttonProjectPath.Margin = new System.Windows.Forms.Padding(4);
            this.buttonProjectPath.Name = "buttonProjectPath";
            this.buttonProjectPath.Size = new System.Drawing.Size(96, 29);
            this.buttonProjectPath.TabIndex = 2;
            this.buttonProjectPath.Text = "Browse";
            this.buttonProjectPath.UseVisualStyleBackColor = true;
            this.buttonProjectPath.Click += new System.EventHandler(this.buttonProjectPath_Click);
            // 
            // labelClassName
            // 
            this.labelClassName.AutoSize = true;
            this.labelClassName.Location = new System.Drawing.Point(46, 97);
            this.labelClassName.Name = "labelClassName";
            this.labelClassName.Size = new System.Drawing.Size(86, 19);
            this.labelClassName.TabIndex = 3;
            this.labelClassName.Text = "Class Name:";
            // 
            // textBoxClassName
            // 
            this.textBoxClassName.Location = new System.Drawing.Point(157, 94);
            this.textBoxClassName.Name = "textBoxClassName";
            this.textBoxClassName.Size = new System.Drawing.Size(155, 26);
            this.textBoxClassName.TabIndex = 4;
            // 
            // labelMethodName
            // 
            this.labelMethodName.AutoSize = true;
            this.labelMethodName.Location = new System.Drawing.Point(389, 97);
            this.labelMethodName.Name = "labelMethodName";
            this.labelMethodName.Size = new System.Drawing.Size(101, 19);
            this.labelMethodName.TabIndex = 5;
            this.labelMethodName.Text = "Method Name:";
            // 
            // textBoxMethodName
            // 
            this.textBoxMethodName.Location = new System.Drawing.Point(500, 94);
            this.textBoxMethodName.Name = "textBoxMethodName";
            this.textBoxMethodName.Size = new System.Drawing.Size(155, 26);
            this.textBoxMethodName.TabIndex = 6;
            // 
            // labelPathConstraint
            // 
            this.labelPathConstraint.AutoSize = true;
            this.labelPathConstraint.Location = new System.Drawing.Point(46, 149);
            this.labelPathConstraint.Name = "labelPathConstraint";
            this.labelPathConstraint.Size = new System.Drawing.Size(105, 19);
            this.labelPathConstraint.TabIndex = 7;
            this.labelPathConstraint.Text = "Path Constraint:";
            // 
            // textBoxPathConstraint
            // 
            this.textBoxPathConstraint.Location = new System.Drawing.Point(157, 146);
            this.textBoxPathConstraint.Name = "textBoxPathConstraint";
            this.textBoxPathConstraint.Size = new System.Drawing.Size(499, 26);
            this.textBoxPathConstraint.TabIndex = 8;
            // 
            // buttonPathConstraint
            // 
            this.buttonPathConstraint.Location = new System.Drawing.Point(664, 144);
            this.buttonPathConstraint.Name = "buttonPathConstraint";
            this.buttonPathConstraint.Size = new System.Drawing.Size(96, 29);
            this.buttonPathConstraint.TabIndex = 9;
            this.buttonPathConstraint.Text = "Browse";
            this.buttonPathConstraint.UseVisualStyleBackColor = true;
            this.buttonPathConstraint.Click += new System.EventHandler(this.buttonPathConstraint_Click);
            // 
            // buttonSolve
            // 
            this.buttonSolve.Location = new System.Drawing.Point(354, 214);
            this.buttonSolve.Name = "buttonSolve";
            this.buttonSolve.Size = new System.Drawing.Size(96, 43);
            this.buttonSolve.TabIndex = 10;
            this.buttonSolve.Text = "Solve";
            this.buttonSolve.UseVisualStyleBackColor = true;
            this.buttonSolve.Click += new System.EventHandler(this.buttonSolve_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 292);
            this.Controls.Add(this.buttonSolve);
            this.Controls.Add(this.buttonPathConstraint);
            this.Controls.Add(this.textBoxPathConstraint);
            this.Controls.Add(this.labelPathConstraint);
            this.Controls.Add(this.textBoxMethodName);
            this.Controls.Add(this.labelMethodName);
            this.Controls.Add(this.textBoxClassName);
            this.Controls.Add(this.labelClassName);
            this.Controls.Add(this.buttonProjectPath);
            this.Controls.Add(this.textBoxProjectPath);
            this.Controls.Add(this.labelProjectPath);
            this.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DomainSolver";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProjectPath;
        private System.Windows.Forms.TextBox textBoxProjectPath;
        private System.Windows.Forms.Button buttonProjectPath;
        private System.Windows.Forms.Label labelClassName;
        private System.Windows.Forms.TextBox textBoxClassName;
        private System.Windows.Forms.Label labelMethodName;
        private System.Windows.Forms.TextBox textBoxMethodName;
        private System.Windows.Forms.Label labelPathConstraint;
        private System.Windows.Forms.TextBox textBoxPathConstraint;
        private System.Windows.Forms.Button buttonPathConstraint;
        private System.Windows.Forms.Button buttonSolve;
    }
}

