namespace Tools
{
    public delegate void GenericEventHandler();
    public delegate void GenericEventHandler<T>(T t);
    public delegate void GenericEventHandler<T, U>(T t, U u);
    public delegate void GenericEventHandler<T, U, V>(T t, U u, V v);
}
