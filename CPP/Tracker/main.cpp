#include <unistd.h>
#include <sys/signal.h>
#include <sys/wait.h>
#include <sys/ptrace.h>

#include <assert.h>

#include <bits/wordsize.h>
#include <sys/syscall.h>

#include <sys/reg.h>

#include <errno.h>
#include <string.h>
#include <limits.h>
#include <stdio.h>

int main(int argc, char **argv)
{
    pid_t pid;
    if ((pid = ::fork()) == 0)
    {
        // http://man7.org/linux/man-pages/man2/ptrace.2.html
        // If the PTRACE_O_TRACEEXEC option is not in effect, all successful
        // calls to execve(2) by the traced process will cause it to be sent a
        // SIGTRAP signal, giving the parent a chance to gain control before the
        // new program begins execution.
        ::ptrace(PTRACE_TRACEME, 0L, 0L, 0L);

        return ::execlp("cat", "cat", "/usr/include/stdio.h", NULL);
    }
    else
    {
        int status;
        ::waitpid(pid, &status, 0);
        assert(WIFSTOPPED(status));

        //ptrace only works when tracee is stopped?
        int i = ::ptrace(PTRACE_SETOPTIONS, pid, 0L, PTRACE_O_TRACESYSGOOD);

        for (int _7 = 0; _7 < 150; ++_7)
        {
            // http://man7.org/linux/man-pages/man2/ptrace.2.html
            // Restart the stopped tracee as for PTRACE_CONT.
            i = ::ptrace(PTRACE_SYSCALL, pid, 0L, 0L);
            auto str2 = strerror(errno);

            //Syscall Entry
            //int status;
            auto p2 = ::waitpid(pid, &status, 0);
            bool isstopped = (WIFSTOPPED(status));
            auto stopsig = (WSTOPSIG(status));

            if ((WIFSTOPPED(status)) && ((WSTOPSIG(status)) == (SIGTRAP | 0X80)))
            {
#ifdef __x86_64__
                // http://man7.org/linux/man-pages/man2/syscall.2.html
                // System call #
                // rax
                auto systemcallnumber_entry = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * ORIG_RAX, 0L);
#elif defined(__i386__)
                //Compiler May Define Both __x86_64__ and __i386__ Because Of Bugs.

#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif
                switch (systemcallnumber_entry)
                {
                case SYS_open:
                {
#ifdef __x86_64__
                    // http://man7.org/linux/man-pages/man2/syscall.2.html
                    // arg1
                    // rdi
                    auto arg1 = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * RDI, 0L);
#elif defined(__i386__)

#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif
                    char pathname[4096];
                    for (int _w = 0; _w < (4096 / (__WORDSIZE / CHAR_BIT)); ++_w)
                    {
                        auto val4 = ::ptrace(PTRACE_PEEKTEXT, pid, arg1 + (__WORDSIZE / CHAR_BIT) * _w, 0L);

                        bool meetzeroterm = false;
                        for (int _c = 0; _c < (__WORDSIZE / CHAR_BIT); ++_c)
                        {
                            char val = reinterpret_cast<char *>(&val4)[_c];
                            pathname[(__WORDSIZE / CHAR_BIT) * _w + _c] = val;
                            if (val == '\0')
                            {
                                meetzeroterm = true;
                                break;
                            }
                        }

                        if (meetzeroterm)
                        {
                            break;
                        }
                    }
                    printf("track - %s\n", pathname);
                }
                break;
                case SYS_openat:
                    break;
                default:
                    break;
                }

                // Restart the stopped tracee as for PTRACE_CONT.
                i = ::ptrace(PTRACE_SYSCALL, pid, 0L, 0L);

                //Syscall Exit //Some Syscall(such as exec) not return?
                p2 = ::waitpid(pid, &status, 0);
                bool isstopped = (WIFSTOPPED(status));
                bool isexited = (WIFEXITED(status));

                if ((WIFEXITED(status)))
                {
                    break;
                }

#ifdef __x86_64__
                auto systemcallnumber_exit = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * ORIG_RAX, 0L);
#elif defined(__i386__)
#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif
                assert(systemcallnumber_entry == systemcallnumber_exit);
            }
            else if ((WIFEXITED(status)))
            {
                break;
            }
            else
            {
                i = ::ptrace(PTRACE_SYSCALL, pid, 0L, 0L);
                p2 = ::waitpid(pid, &status, 0);
                bool isstopped = (WIFSTOPPED(status));
                bool isexited = (WIFEXITED(status));
            }
        }

        return 0;
    }
}
