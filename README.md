Overview
========

Beanstalk-sharp is a C#/.NET client library for [beanstalkd](http://github.com/kr/beanstalkd). Currently it only implements commands to produce new messages.

Example:

    using (var beanstalk = new BeanstalkConnection(IPAddress.Loopback, 11300))
    {
        beanstalk.Put("hello");                
    }

To do
=====

* Add commands for consuming messages