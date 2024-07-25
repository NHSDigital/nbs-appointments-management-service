const Layout = ({
    children,
  }: Readonly<{
    children: React.ReactNode;
  }>) => (
    <div className="box-content p-4">
        {children}
    </div>
)

export default Layout